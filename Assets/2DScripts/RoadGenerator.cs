using UnityEngine;
using System.Collections.Generic;
using WebSocketSharp;
using System.Collections.Generic;
using System.Collections.Concurrent;
using WebSocketTraffic;
using System;

namespace Concurrent {
    public class RoadGenerator : MonoBehaviour
{   

    // defining the prefabs
    public GameObject oneLane;
    public GameObject twoLane;
    public GameObject threeLane;

    public GameObject bus;
    public GameObject conventionalCar;
    public GameObject electricCar;

    public GameObject intersection;
    public GameObject bgPrefab;

    // defining the websocket & simulation params
    private WebSocket ws;
    public float vehicleDensity, autoFlowPercent, mapSize;
    public int selectedIndex;
    public bool fullDay, receiveNewDests, graphics, roadBlockage;

    // dictionaries to store the vehicles, roads and intersections
    public Dictionary<int, Vehicle2D> vehicles = new Dictionary<int, Vehicle2D>();
    public Dictionary<int, RoadInitMsg> roads = new Dictionary<int, RoadInitMsg>();
    public Dictionary<int, VirtualInt> intersections = new Dictionary<int, VirtualInt>();
    public bool vehiclesSpawned = false;
    public AllRoads roadss;



    // a thread-safe queue to store actions to be executed on the Unity main thread
    private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>(); 
    
    // fill the background with a plane of "grass"
    void populateBG() {
        GameObject bgSegment = Instantiate(bgPrefab, new Vector3(0, -1f, 0), Quaternion.identity);
        bgSegment.transform.rotation = Quaternion.Euler(90, 0, 0);
        bgSegment.transform.localScale = new Vector3(1000000, 1000000, 1000000);
    }

    // helper function to spawn all virtual intersections
    void spawnVirtualIntersections(List<VirtualIntMsg> virtualInts, Vector3 startPosition, Vector3 endPosition, int lanecount) {
        
        // centre dist is the distance between each lane, and base dist is the offset of the 1st lane from the middle of the road
        // they are different for each number of lanes
        float centreDist3 = 1.05f;
        float baseDist3 = 0.5f;
        float centreDist2 = 1.2f;
        float baseDist2 = 0.7f;
        float centreDist1 = 0f;
        float baseDist1 = 0.8f;
        float baseDist, centreDist;

        if (lanecount == 3) {
            baseDist = baseDist3;
            centreDist = centreDist3;
        } else if (lanecount == 2) {
            baseDist = baseDist2;
            centreDist = centreDist2;
        } else {
            baseDist = baseDist1;
            centreDist = centreDist1;
        }


        Vector3 direction = (endPosition - startPosition);
        float length = Vector3.Distance(startPosition, endPosition);
        
        // rotate 90 degrees anticlockwise
        Vector3 directionLeft = new Vector3(-direction.z, direction.y, direction.x) / direction.magnitude;
        float zRot = -Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        
        // rotate 90 degrees clockwise
        Vector3 directionRight = new Vector3(direction.z, direction.y, -direction.x) / direction.magnitude;
        for (int i = 0; i < virtualInts.Count; i++) {
            
            // the lane order is set up like 012210
            VirtualIntMsg virtualInt = virtualInts[i];

            // set the position, direction and lane of the virtual intersection
            float position, dir, lane;
            position = virtualInt.position;
            dir = virtualInt.direction;
            lane = virtualInt.lane;
            int trueLane = (int)lane;
            // trueLane is its true offset from the middle of the road
            trueLane = lanecount - trueLane - 1;
            
            // determine the position of the virtual intersection
            Vector3 pos;
            if (dir == 1) {
                pos = startPosition + direction * position + directionLeft * (baseDist + centreDist * trueLane);
            } else {
                pos = startPosition + direction * position + directionRight * (baseDist + centreDist * trueLane);
            }
            
            // instantiate the virtual intersection
            GameObject inter = Instantiate(intersection, pos, Quaternion.identity);
            inter.transform.rotation = Quaternion.Euler(90, 0, zRot);
            // make invisible
            inter.transform.localScale *= 0.01f;

            // create the new virtualInt as a script component of the GameObject
            VirtualInt currentInt = inter.GetComponent<VirtualInt>();
            currentInt.id = virtualInt.id;
            currentInt.x = pos.x;
            currentInt.y = pos.z;
            currentInt.lane = (int)lane;
            currentInt.direction = (int)dir;
            currentInt.position = position;
            currentInt.trafficLightOrder = virtualInt.trafficLightOrder;
            currentInt.lightTiming = virtualInt.lightTiming;
            currentInt.activeLight = -1;
            currentInt.road = virtualInt.road;

            intersections.Add(virtualInt.id, currentInt);
            
        }
    }

    // spawn a road segment
    void spawnRoadSegment(GameObject roadSegment, RoadInitMsg msg, int index) {
        // Create a parent GameObject
        Vector3 startPosition = msg.RealStartPos;
        Vector3 endPosition = msg.RealEndPos;
        int lanecount = msg.laneCount;
        GameObject parent = new GameObject("Road" + index);

        // since each road is 1 unit long, this is how many segments we need
        float length = Vector3.Distance(startPosition, endPosition);
        int lengthInt = (int)length;
        Vector3 direction = (endPosition - startPosition) / lengthInt;
        
        float zRot = -Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        
        // create two roads at either end of the road segment to make it look cohesive
        GameObject road1 = Instantiate(roadSegment, startPosition - direction, Quaternion.identity);
        road1.transform.rotation = Quaternion.Euler(90, 0, zRot);
        road1.transform.parent = parent.transform; // Set the parent of the road segment

        GameObject road2 = Instantiate(roadSegment, endPosition + direction, Quaternion.identity);
        road2.transform.rotation = Quaternion.Euler(90, 0, zRot);
        road2.transform.parent = parent.transform; // Set the parent of the road segment
        
        // create the rest of the road segments (this does double create the extra two at the extreme ends)
        for (int i = -1; i <= lengthInt; i++) {
            GameObject road = Instantiate(roadSegment, startPosition + direction * i, Quaternion.identity);
            road.transform.rotation = Quaternion.Euler(90, 0, zRot);
            road.transform.parent = parent.transform; // Set the parent of the road segment
            
        }

        // Add the road to the roads dictionary
        roads.Add(msg.id, msg);
    }

    // method to spawn a vehicle
    void spawnVehicle(VehicleInitMsg vehicle) {
        int type = vehicle.type;
        GameObject vehicleType;
        
        // create its prefab based on type
        if (type == 0) {
            vehicleType = conventionalCar;
        } else if (type == 1) {
            vehicleType = electricCar;
        } else {
            vehicleType = bus;
        }

        // set the spawn position and rotation
        int spawnID = vehicle.position.id;
        VirtualInt spawn = intersections[spawnID];
        float x = spawn.x;
        float y = spawn.y;

        float zRot = roads[spawn.road].zRot;

        // replace every route int with the new, good virtint object instead of the virtintinit
        List<VirtualInt> newRoute = new List<VirtualInt>();
        foreach (VirtualIntMsg virtInt in vehicle.route) {
            newRoute.Add(intersections[virtInt.id]);
            if (intersections[virtInt.id].id != virtInt.id) {
                Debug.Log("Error: virtInt.id != virtInt.id");
            }
            //Debug.Log(intersections[virtInt.id].x + " " + intersections[virtInt.id].y);
        }

        // instantiate the vehicle
        GameObject vehicle1 = Instantiate(vehicleType, new Vector3(x, 1f, y), Quaternion.identity);
        vehicle1.transform.rotation = Quaternion.Euler(90, 0, zRot);

        Vehicle2D currentVehicle = vehicle1.GetComponent<Vehicle2D>();
        currentVehicle.id = vehicle.id;
        currentVehicle.emissionRate = vehicle.emissionRate;
        currentVehicle.type = vehicle.type;
        currentVehicle.route = newRoute;
        currentVehicle.position = spawn;
        currentVehicle.spawn = spawn;

        vehicles.Add(vehicle.id, currentVehicle);

    }

    // method to spawn an intersection (not used right now)
    void spawnIntersection(Vector3 position) {
        GameObject inter = Instantiate(intersection, new Vector3(position.x, 1, position.y), Quaternion.identity);
        inter.transform.rotation = Quaternion.Euler(90, 0, 0);
        inter.transform.localScale *= position.z;
    }

    private void Awake() {
        // connect to the server
        ws = new WebSocket("ws://localhost:8001/");
        
        Debug.Log("Connecting to server");
        
        // handle messages from the server
        ws.OnMessage += (sender, e) =>
            HandleMessage(e.Data);
        
        ws.Connect();
        
        Debug.Log("Connected to server");
        
        // retrieve the simulation parameters from the player prefs
        vehicleDensity = PlayerPrefs.GetFloat("vehicleDensity", 0f);
        autoFlowPercent = PlayerPrefs.GetFloat("autoFlowPercent", 0f);
        mapSize = PlayerPrefs.GetFloat("mapSize", 0f);
        selectedIndex = PlayerPrefs.GetInt("selectedIndex", 0);
        fullDay = PlayerPrefs.GetInt("fullDay", 0) == 1;
        receiveNewDests = PlayerPrefs.GetInt("receiveNewDests", 0) == 1;
        graphics = PlayerPrefs.GetInt("graphics", 0) == 1;
        roadBlockage = PlayerPrefs.GetInt("roadBlockage", 0) == 1;

        // send these prefs to the backend
        string generateMsg = "{" +
            "'vehicleDensity':" + vehicleDensity + "," +
            "'autoFlowPercent':" + autoFlowPercent + "," +
            "'mapSize':" + mapSize + "," +
            "'selectedIndex':" + selectedIndex + "," +
            "'fullDay':" + (fullDay ? 1 : 0) + "," +
            "'receiveNewDests':" + (receiveNewDests ? 1 : 0) + "," +
            "'graphics':" + (graphics ? 1 : 0) + "," +
            "'roadBlockage':" + (roadBlockage ? 1 : 0) + "}";
        
        ws.Send(generateMsg);
        Debug.Log("Sent message: " + generateMsg);
        populateBG();
        Debug.Log("Populated BG");
    }

    private void OnDestroy()
    {
        ws.Close();
    }

    private void Update()
    {   
        // work the dispatched actions on the Unity main thread
        while(_actions.Count > 0)
        {
            if(_actions.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }

    void HandleMessage(string message) {
        roadss = JsonUtility.FromJson<AllRoads>(message);

        // spawning roads
        int index = 0;
        foreach (RoadInitMsg road in roadss.roads) {
            GameObject roadSegment = oneLane;
            
            // determine the road segment prefab based on the number of lanes
            if (road.laneCount == 2) {
                roadSegment = twoLane;
            } else if (road.laneCount == 3) {
                roadSegment = threeLane;
            }

            int curh = index;

            // spawn the road segment
            _actions.Enqueue(() => spawnRoadSegment(roadSegment, road, curh));
            index += 1;
            
            // spawn the virtual intersections
            Vector3 startPosition = road.RealStartPos;
            Vector3 endPosition = road.RealEndPos;
            _actions.Enqueue(() => spawnVirtualIntersections(road.virtualInts, startPosition, endPosition, road.laneCount));
        }

        // spawning vehicles
        foreach (VehicleInitMsg vehicle in roadss.vehicles) {
            _actions.Enqueue(() => spawnVehicle(vehicle));
        }

        vehiclesSpawned = true;


    }
}

}