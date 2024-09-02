using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Concurrent
{
    public class Vehicle2D : MonoBehaviour
    {

        // defining the vehicle's properties - their names are generally self-explanatory
        public int id;
        public VirtualInt position;
        public VirtualInt spawn;
        public int type;
        public int lane;

        public float maxSpeed = 25f;
        public List<VirtualInt> route = new();
        public float tolerance = 0.5f; // modify this in prefab inspector
        public float destTolerance = 0.5f;
        public float emissionRate; // Per meter
        public float cumulativeEmission;
        public float tripTime;
        public float idleTime;
        public float accelerationMPSS = 0f;
        public float currentSpeed = 0f;
        public List<VirtualInt> visited = new();

        public VirtualInt prevGoal;
        public int intendedLaneChange = 0;
        public bool laneChangeInitiated = false;

        // polarised positions are the positions moved RELATIVE to the road's centre -> "polarises" any parallel movement
        Vector3 formerPolarisedPos;
        Vector3 currentPolarisedPos;

        public float timeElapsed = 0f;

        // defining the road generator object driving the whole scene, as we need to access the road & virtual intersection data
        RoadGenerator rg;
        

        private void Awake()
        {
            tripTime = 0f;
            idleTime = 0f;
            accelerationMPSS = 5f;
            currentSpeed = 0f;
            maxSpeed = 20f;
            visited = new();
            tolerance = 1f; // modify this in prefab inspector
            destTolerance = 1f;
            rg = GameObject.Find("RoadGenerator").GetComponent<RoadGenerator>();
        }

        // stub function for later use - checks if the vehicle can change into a lane without colliding with another vehicle
        public bool canChangeInto(int lane) {
            return true;
        }

        // function to change the lane of the vehicle
        public void changeLane() {

            // it doesn't need to change lanes
            if (intendedLaneChange == 0) {
                return;
            }

            // defining the distances from the centre of the road for each lane count (the lane widths, essentially)
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

            // defining the desired change in lane & new lane after the lane change
            int desiredChange = intendedLaneChange > 0 ? 1 : -1;
            int newLane = lane + desiredChange;
            int direction = prevGoal.direction;

            // left dir is a 90 degree anticlockwise rotation of the road direction, right dir is a 90 degree clockwise rotation
            Vector3 roadDir = rg.roads[prevGoal.road].direction;
            Vector3 leftDir = new Vector3(-roadDir.z, 0, roadDir.x) / roadDir.magnitude;
            Vector3 rightDir = new Vector3(roadDir.z, 0, -roadDir.x) / roadDir.magnitude;

            // if a lane change hasn't started, initialise the polarised positions for later comparison
            if (!laneChangeInitiated) {

                formerPolarisedPos = transform.position;
                currentPolarisedPos = transform.position;
                laneChangeInitiated = true;

            } else {

                // if the vehicle is moving in direction 1 and wants to increase its lane number OR is moving in direction -1 and wants to decrease its lane number,
                // move right, else move left
                if ((desiredChange > 0 && direction == 1) || (desiredChange < 0 && direction == -1)) {
                    transform.position += rightDir * 10 * Time.deltaTime;
                    currentPolarisedPos += rightDir * 10 * Time.deltaTime;
                } else {
                    transform.position += leftDir * 10 * Time.deltaTime;
                    currentPolarisedPos += leftDir * 10 * Time.deltaTime;
                }

                // if the vehicle has moved the width of the lane, officially change the lane and reset the polarised positions
                // decrement the magnitude of the intended lane change as we have already moved one lane
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

            // increment the trip time
            tripTime += Time.deltaTime;
            
            // define the current position of the vehicle and the next position it should move to, and thus its direction
            Vector3 curPos = transform.position;
            Vector3 nextPos = route[0].RealPosition;
            Vector3 direction = nextPos - curPos + new Vector3(0, 1f, 0);
            rg = GameObject.Find("RoadGenerator").GetComponent<RoadGenerator>();
            
            // toMove represents whether the car is moving or not - if it's not moving, it's waiting at a red light
            bool toMove = true;
            
            // this is what happens if the vehicle has reached its current goal
            if (direction.magnitude < destTolerance) {

                // if the vehicle has reached its final destination, destroy it
                if (route.Count == 1) {
                    Destroy(gameObject);
                    return;
                }

                // if the vehicle has reached a virtual intersection, update the visited list and the current goal
                prevGoal = route[0];
                route.RemoveAt(0);
                nextPos = route[0].RealPosition;

                
                float zRot = transform.rotation.eulerAngles.z;
                
                // if it's the next virtual intersection on the same road, continue in dir. of road and make a lane change if needed
                if (route[0].road == prevGoal.road) {
                    
                    Debug.Log("Next goal is on the same road for vehicle " + id);
                    
                    Vector3 roadDir = rg.roads[prevGoal.road].direction;
                    RoadInitMsg curRoad = rg.roads[prevGoal.road];
                    zRot = curRoad.zRot;
                    
                    // we may need to change lanes, though
                    intendedLaneChange = route[0].lane - prevGoal.lane;

                    // if the vehicle is not moving, don't accelerate
                    transform.rotation = Quaternion.Euler(90, 0, zRot); 
                    if (!toMove) {
                        return;
                    }

                    // otherwise, accelerate and move strictly in the direction of the road
                    if (currentSpeed < maxSpeed) {
                        currentSpeed += accelerationMPSS * Time.deltaTime;
                    }

                    // move in the direction of the road
                    int moveDir = prevGoal.direction;
                    if (moveDir == 1) {
                        transform.position += roadDir.normalized * currentSpeed * Time.deltaTime;
                    } else {
                        transform.position -= roadDir.normalized * currentSpeed * Time.deltaTime;
                    }

                    // increment cumulative emissions and change the lane if needed
                    cumulativeEmission += emissionRate * Time.deltaTime;
                    changeLane();

                } else {
                    // traffic light check

                    // if the traffic light is green OR the vehicle has been waiting for 4 seconds, move past the intersection
                    if (timeElapsed >= 4f || route[0].id == prevGoal.id) {
                        
                        Debug.Log("Traffic light is green for vehicle " + id);

                        // reset the time elapsed and the intended lane change, and accelerate.
                        timeElapsed = 0f;
                        intendedLaneChange = 0;
                        lane = route[0].lane;
                        direction = nextPos - curPos + new Vector3(0, 1f, 0);
                        if (currentSpeed < maxSpeed) {
                            currentSpeed += accelerationMPSS * Time.deltaTime;
                        }
                        cumulativeEmission += emissionRate * Time.deltaTime;
                        transform.position += direction.normalized * currentSpeed * Time.deltaTime;
                    
                    } else {

                        // if the traffic light is red, increment the idle time and the cumulative emissions, and don't move
                        Debug.Log("Traffic light is red for vehicle " + id);
                        idleTime += Time.deltaTime;

                        // idle emissions = 2x normal emissions
                        cumulativeEmission += 2 * emissionRate * Time.deltaTime;
                        timeElapsed += Time.deltaTime;
                        toMove = false;
                        direction = nextPos - curPos + new Vector3(0, 1f, 0);
                        
                        // add prevGoal back to route to reset the state of the vehicle at the next iteration
                        route.Insert(0, prevGoal);
                    }

                    // make the vehicle face the direction it's moving in
                    zRot = -Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(90, 0, zRot);
                    
                }
                
                //Debug.Log("Next goal: " + route[0].id);
            } else {

                // making sure to not double count emissions
                cumulativeEmission += emissionRate * Time.deltaTime * currentSpeed;
            }


            
            // same cases again - (1) if the goal is on the same road, (2) moving between roads
            if (route[0].road == prevGoal.road){
                
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
                if (currentSpeed < maxSpeed) {
                    currentSpeed += accelerationMPSS * Time.deltaTime;
                }
                int moveDir = prevGoal.direction;
                if (moveDir == 1) {
                    transform.position += roadDir.normalized * currentSpeed * Time.deltaTime;
                } else {
                    transform.position -= roadDir.normalized * currentSpeed * Time.deltaTime;
                }

            } else {

                direction = nextPos - curPos + new Vector3(0, 1f, 0);
                if (currentSpeed < maxSpeed) {
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