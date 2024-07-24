using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;

namespace WebSocketTraffic {

    public class WebsocketManager : MonoBehaviour
    {
        public bool websocketHasInitialized = false;
        public InitialSpawner initialSpawner;
        public VehicleManager vehicleManager;

        public SimConfig simConfig;
        private WebSocket ws;
        public bool amUpdating = false;


        public float updateInterval = 5.0f;

        public float vehicleDensity, autoFlowPercent, mapSize;
        public int selectedIndex;
        public bool fullDay, receiveNewDests, graphics, roadBlockage;

        // Awake is called on loading -> gets simulation configuration from player prefs
        private void Awake()
        {
            ws = new WebSocket("ws://localhost:8001/");
            ws.OnMessage += (sender, e) =>
                HandleMessage(e.Data);
            ws.Connect();

            vehicleDensity = PlayerPrefs.GetFloat("vehicleDensity", 0f);
            autoFlowPercent = PlayerPrefs.GetFloat("autoFlowPercent", 0f);
            mapSize = PlayerPrefs.GetFloat("mapSize", 0f);
            selectedIndex = PlayerPrefs.GetInt("selectedIndex", 0);
            fullDay = PlayerPrefs.GetInt("fullDay", 0) == 1;
            receiveNewDests = PlayerPrefs.GetInt("receiveNewDests", 0) == 1;
            graphics = PlayerPrefs.GetInt("graphics", 0) == 1;
            roadBlockage = PlayerPrefs.GetInt("roadBlockage", 0) == 1;

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

            // Send information every n seconds
            InvokeRepeating("SendInfo", 10.0f, updateInterval);
        }

        void OnEnable() {
            
        }

        

        private void SendInfo()
        {
            var vehicles = vehicleManager.vehicles;
            string updatemsg = "{";

            foreach (var pair in vehicles)
            {  
                var vehicle = pair.Value;
                updatemsg += vehicle.id + ":{'Metadata':[";
                updatemsg += vehicle.transform.position.x + "," + vehicle.transform.position.z + "," + vehicle.currentRoadId + "],'Routes':[";
                foreach (var routenode in vehicle.route)
                {
                    updatemsg += "(" + routenode.x + "," + routenode.y + "," + routenode.z + "),";
                }
                updatemsg += "]},";
                //Debug.Log(infor[0] + " " + infor[1]);
            }

            updatemsg += "}";

            ws.Send(updatemsg);
        }

        private void Reset()
        {
            initialSpawner = GetComponent<InitialSpawner>();
            vehicleManager = GetComponent<VehicleManager>();
        }

        private void OnDestroy()
        {
            ws.Close();
        }

        private void HandleMessage(string jsonMsg)
        {
            if (websocketHasInitialized) {
                var updateMsg = JsonUtility.FromJson<UpdateMessage>(jsonMsg);
                if (amUpdating) vehicleManager.isUpdating = true;
                vehicleManager.HandleUpdateMessage(updateMsg);
                amUpdating = true;
                Debug.Log("update");
            } else {
                var initMsg = JsonUtility.FromJson<InitMessage>(jsonMsg);
                updateInterval = (float)initMsg.vehicles[0].id;
                Debug.Log(updateInterval);
                initMsg.vehicles.RemoveAt(0);

                initialSpawner.HandleInitMessage(initMsg);
                websocketHasInitialized = true;
                initialSpawner.timeToSpawn = true;
                Debug.Log("init");
            }
        }
    }
}