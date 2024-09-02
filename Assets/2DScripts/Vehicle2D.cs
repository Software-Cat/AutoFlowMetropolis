using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Concurrent
{
    public class Vehicle2D : MonoBehaviour
    {
        public int id;
        public VirtualInt position;

        public VirtualInt spawn;
        public int type;
        public int lane;

        public float nextRoadLength;
        public int nextRoadCarCount;
        public int nextRoadBusCount; 
        public bool nextRoadFull;
        public bool nextGoalOccupied = false;
        public bool allowedRoad = false;
        public bool hasBeenQueued = false;

        public int currentRoadId = -1;

        public float deadTime = 0f;

        // public float positionOnRoad;
        public float speed = 25f;
        public List<VirtualInt> route = new();
        public float rotSpeed = 10f;
        public float tolerance = 0.5f; // modify this in prefab inspector
        public float destTolerance = 0.5f;
        public List<Transform> raycastPoints;
        public float inFrontThreshold = 1f; // modify this in prefab inspector
        public BoxCollider boxCollider;
        public Rigidbody rb;
        public float emissionRate; // Per meter
        public float cumulativeEmission;
        public float tripTime;
        public bool useAutoFlow;
        public float idleTime;
        public bool blockedByIntersection;
        //public Intersection currentIntersection;
        //public TrafficLights trafficLights;
        public bool bypassCollisions;
        public float currentSessionIdleTime;
        public int passengerCount;
        public LineRenderer lineRenderer;
        public bool drawPathLine;
        public int substepsPerTick = 3;
        public float accelerationMPSS = 0f;
        public float currentSpeed = 0f;
        public List<VirtualInt> visited = new();

        public VirtualInt prevGoal;
        public bool drawn = false;
        public int intendedLaneChange = 0;
        public bool laneChangeInitiated = false;
        Vector3 formerPolarisedPos;
        Vector3 currentPolarisedPos;

        public float timeElapsed = 0f;

        RoadGenerator rg;
        

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            boxCollider = GetComponent<BoxCollider>();
            tripTime = 0f;
            idleTime = 0f;
            blockedByIntersection = false;
            bypassCollisions = false;
            currentSessionIdleTime = 0f;
            lineRenderer = GetComponent<LineRenderer>();
            drawPathLine = false;
            accelerationMPSS = 5f;
            currentSpeed = 0f;
            speed = 20f;
            visited = new();
            tolerance = 1f; // modify this in prefab inspector
            destTolerance = 1f;
            rg = GameObject.Find("RoadGenerator").GetComponent<RoadGenerator>();
        }

        public bool canChangeInto(int lane) {
            return true;
        }

        public void changeLane() {
            if (intendedLaneChange == 0) {
                return;
            }
            float centreDist3 = 1.05f;
            float centreDist2 = 1.2f;
            float centreDist1 = 0f;
            float baseDist, centreDist;
            int lanecount = rg.roads[prevGoal.road].laneCount;

            if (lanecount == 3) {
                centreDist = centreDist3;
            } else if (lanecount == 2) {
                centreDist = centreDist2;
            } else {
                centreDist = centreDist1;
            }
            int desiredChange = intendedLaneChange > 0 ? 1 : -1;
            int newLane = lane + desiredChange;
            int direction = prevGoal.direction;
            Vector3 roadDir = rg.roads[prevGoal.road].direction;
            Vector3 leftDir = new Vector3(-roadDir.z, 0, roadDir.x) / roadDir.magnitude;
            Vector3 rightDir = new Vector3(roadDir.z, 0, -roadDir.x) / roadDir.magnitude;

            if (!laneChangeInitiated) {
                formerPolarisedPos = transform.position;
                currentPolarisedPos = transform.position;
                laneChangeInitiated = true;
            } else {
                if ((desiredChange > 0 && direction == 1) || (desiredChange < 0 && direction == -1)) {
                    transform.position += rightDir * 10 * Time.deltaTime;
                    currentPolarisedPos += rightDir * 10 * Time.deltaTime;

                } else {
                    transform.position += leftDir * 10 * Time.deltaTime;
                    currentPolarisedPos += leftDir * 10 * Time.deltaTime;
                }
                if (Vector3.Distance(currentPolarisedPos, formerPolarisedPos) >= centreDist) {
                    lane = newLane;
                    laneChangeInitiated = false;
                    if (intendedLaneChange > 0) {
                        intendedLaneChange--;
                    } else {
                        intendedLaneChange++;
                    }
                }
            }
        }


        private void Update() {
            tripTime += Time.deltaTime;
            Vector3 curPos = transform.position;
            //Debug.Log("Current position: " + curPos);
            Vector3 nextPos = route[0].RealPosition;
            Vector3 direction = nextPos - curPos + new Vector3(0, 1f, 0);
            rg = GameObject.Find("RoadGenerator").GetComponent<RoadGenerator>();
            //Debug.Log("Direction: " + direction.magnitude);
            bool toMove = true;

            if (direction.magnitude < destTolerance) {
                if (route.Count == 1) {
                    Destroy(gameObject);
                    return;
                }
                prevGoal = route[0];
                route.RemoveAt(0);
                nextPos = route[0].RealPosition;

                // if it's the next vint on the same road, continue in dir. of road and make a lane change if needed
                float zRot = transform.rotation.eulerAngles.z;
                if (route[0].road == prevGoal.road) {
                    Debug.Log("Next goal is on the same road for vehicle " + id);
                    Vector3 roadDir = rg.roads[prevGoal.road].direction;
                    RoadInitMsg curRoad = rg.roads[prevGoal.road];
                    zRot = curRoad.zRot;
                    if (route[0].lane != prevGoal.lane) {
                        //Debug.Log("Lane change");
                        intendedLaneChange = route[0].lane - prevGoal.lane;
                    }
                    transform.rotation = Quaternion.Euler(90, 0, zRot); 
                    if (!toMove) {
                        return;
                    }
                    if (currentSpeed < speed) {
                        currentSpeed += accelerationMPSS * Time.deltaTime;
                    }
                    int moveDir = prevGoal.direction;
                    if (moveDir == 1) {
                        transform.position += roadDir.normalized * currentSpeed * Time.deltaTime;
                    } else {
                        transform.position -= roadDir.normalized * currentSpeed * Time.deltaTime;
                    }
                    cumulativeEmission += emissionRate * Time.deltaTime  * currentSpeed;
                    
                    changeLane();

                } else {
                    //Debug.Log(rg.intersections[prevGoal.id].activeLight + " " + rg.intersections[route[0].id].activeLight);
                    //Debug.Log(route[0].id + " " + prevGoal.id);
                    // traffic light check
                    if (timeElapsed >= 4f || route[0].id == prevGoal.id) {
                        Debug.Log("Traffic light is green for vehicle " + id);
                        timeElapsed = 0f;
                        intendedLaneChange = 0;
                        lane = route[0].lane;
                        direction = nextPos - curPos + new Vector3(0, 1f, 0);
                        if (currentSpeed < speed) {
                            currentSpeed += accelerationMPSS * Time.deltaTime;
                        }
                        cumulativeEmission += emissionRate * Time.deltaTime  * currentSpeed;
                        transform.position += direction.normalized * currentSpeed * Time.deltaTime;
                    } else {
                        Debug.Log("Traffic light is red for vehicle " + id);
                        idleTime += Time.deltaTime;
                        cumulativeEmission += 2 * emissionRate * Time.deltaTime  * currentSpeed;
                        timeElapsed += Time.deltaTime;
                        toMove = false;
                        direction = nextPos - curPos + new Vector3(0, 1f, 0);
                        // add prevGoal back to route
                        route.Insert(0, prevGoal);
                    }
                    zRot = -Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(90, 0, zRot);
                    
                }
                
                //Debug.Log("Next goal: " + route[0].id);
            }

            cumulativeEmission += emissionRate * Time.deltaTime * currentSpeed;
            if (route[0].road == prevGoal.road){
                Debug.Log("Next goal is on the same road for vehicle " + id);
                Vector3 roadDir = rg.roads[prevGoal.road].direction;
                RoadInitMsg curRoad = rg.roads[prevGoal.road];
                float zRot = curRoad.zRot;
                if (route[0].lane != prevGoal.lane) {
                    //Debug.Log("Lane change");
                    intendedLaneChange = route[0].lane - prevGoal.lane;
                }
                transform.rotation = Quaternion.Euler(90, 0, zRot); 
                if (!toMove) {
                    return;
                }
                if (currentSpeed < speed) {
                    currentSpeed += accelerationMPSS * Time.deltaTime;
                }
                int moveDir = prevGoal.direction;
                if (moveDir == 1) {
                    transform.position += roadDir.normalized * currentSpeed * Time.deltaTime;
                } else {
                    transform.position -= roadDir.normalized * currentSpeed * Time.deltaTime;
                }
                changeLane();

            } else {
                direction = nextPos - curPos + new Vector3(0, 1f, 0);
                if (currentSpeed < speed) {
                    currentSpeed += accelerationMPSS * Time.deltaTime;
                }
                transform.position += direction.normalized * currentSpeed * Time.deltaTime;
                float zRot = -Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(90, 0, zRot);
                
            }




            

            //Debug.Log(Time.deltaTime);

        }


        
    }
}