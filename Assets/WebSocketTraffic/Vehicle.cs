using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace WebSocketTraffic
{
    public class Vehicle : MonoBehaviour
    {
        public int id;
        public Vector3 goal;

        public Vector3 spawn;

        public Vector3 location;

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
        public float speed = 5f;
        public List<Vector3> route = new();
        public float rotSpeed = 10f;
        public float tolerance = 1.5f; // modify this in prefab inspector
        public float destTolerance = 5f;
        public RoadManager roadManager;
        public GameObject tickMark;
        public List<Transform> raycastPoints;
        public float inFrontThreshold = 1f; // modify this in prefab inspector
        public BoxCollider boxCollider;
        public VehicleCustomizer vehicleCustomizer;
        public Rigidbody rb;
        public bool running;
        public float emissionRate; // Per meter
        public float cumulativeEmission;
        public float tripTime;
        public bool useAutoFlow;
        public float idleTime;
        public bool blockedByIntersection;
        public Intersection currentIntersection;
        public TrafficLights trafficLights;
        public bool bypassCollisions;
        public float currentSessionIdleTime;
        public int passengerCount;
        public LineRenderer lineRenderer;
        public bool drawPathLine;
        public int substepsPerTick = 3;
        public float accelerationMPSS = 0f;
        public float currentSpeed = 0f;

        private int carMask;

        public List<Vector3> visited = new();

        public Vector3 prevGoal;

        public bool systemRunning = false;

        public Vector3 NextGoal
        {
            get
            {   
                return route.Count switch
                {
                    0 => Vector3.zero,
                    1 => new Vector3(route[0].x, 0, route[0].y),
                    _ => new Vector3(route[1].x, 0, route[1].y)
                };
            }
        }

        public int NextRoadId
        {
            get
            {
                //return route.Count switch
                //{
                //    0 => -1,
                //    1 => (int)route[0].z,
                //    _ => (int)route[1].z
                //};
                return route.Count switch
                {
                    0 => -1,
                    _ => (int)route[0].z
                };
            }
        }

         public void scaleSize(float scale)
        {
            transform.localScale = new Vector3(scale * transform.localScale.x, scale * transform.localScale.y, scale * transform.localScale.z);
        }

        private void Awake()
        {
            carMask = 1 << LayerMask.NameToLayer("AutonomousVehicle");
            // random scaling factor
            float scale = UnityEngine.Random.Range(0.9f, 1.1f);
            scaleSize(scale);
            accelerationMPSS = UnityEngine.Random.Range(5f, 20f);
        }

        private void Reset()
        {
            boxCollider = GetComponent<BoxCollider>();
            vehicleCustomizer = GetComponent<VehicleCustomizer>();
            rb = GetComponent<Rigidbody>();
            trafficLights = GetComponent<TrafficLights>();
            lineRenderer = GetComponent<LineRenderer>();
        }

       

        private void FixedUpdate()
        {
            // Before route given or after route finished

            if (route.Count == 0 && systemRunning)
            {
                deadTime += Time.deltaTime;
                if (deadTime >= 10f) {
                    gameObject.SetActive(false);
                    return;
                }
                tickMark.SetActive(true);
                boxCollider.enabled = false;
                vehicleCustomizer.GhostMode();
                rb.isKinematic = true;
                currentRoadId = -1;
                return;
            }

            if (!running) return;
            

            // Route just finished
            

            

            location = transform.position;

            // Cosmetics
            UpdateIntersectionLights();
            if (drawPathLine) DrawPath();

            // Next node
            if (ReachedGoal()) OnReachGoal();

            // Blocked by car in front or traffic light
            if (blockedByIntersection || DetectObstacleByRaycast())
            {
                idleTime += Time.deltaTime;
                //tripTime += Time.deltaTime * passengerCount;
                //cumulativeEmission += 2 * speed * Time.deltaTime * emissionRate; // idle emission
                return;
            }
            // var curRoad = roadManager.roads[currentRoadId];

            // if (Math.Abs(location.x - curRoad.RealStartPos.x) < tolerance && Math.Abs(location.x - curRoad.RealEndPos.x) < tolerance) {
            //     positionOnRoad = Math.Abs(location.z - curRoad.RealStartPos.z) / curRoad.Length;
            // } else if (Math.Abs(location.z - curRoad.RealStartPos.z) < tolerance && Math.Abs(location.z - curRoad.RealEndPos.z) < tolerance) {
            //     positionOnRoad = Math.Abs(location.x - curRoad.RealStartPos.x) / curRoad.Length;
            // } else {
            //     positionOnRoad = -1f;
            // }

            tripTime += Time.deltaTime;
            cumulativeEmission += speed * Time.deltaTime * emissionRate;

            // Directions
            var position = transform.position;
            var dirToGoal = (goal - position).normalized;
            var targetRotation = Quaternion.LookRotation(dirToGoal);

            // Is moving
            if (!DetectObstacleByRaycast() || bypassCollisions)
            {
                for (var i = 0; i < substepsPerTick; i++)
                {
                    // Face goal
                    //var degree = Vector3.Angle(transform.forward, dirToGoal);
                    //if (degree > 100f && tripTime > 1f)
                    //{
                    //    Debug.Log($"{transform.forward}, {transform.rotation}, {dirToGoal}, {targetRotation}");
                    //    Debug.LogError($"Car ID: {id}, {degree} degree turn");
                    //    Debug.Break();
                    //}
                    //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    //    rotSpeed * Time.deltaTime / substepsPerTick);
                    transform.rotation = targetRotation;
                    
                    if (route.Count == 0) return;

                    // if (route[0].z != currentRoadId)
                    // {
                    //     transform.rotation = targetRotation;
                    // } else {
                    //     var road = roadManager.roads[currentRoadId];
                    //     var roadDir = (road.RealEndPos - road.RealStartPos).normalized;
                    //     var roadRotation = Quaternion.LookRotation(roadDir);
                    //     transform.rotation = roadRotation;
                    // }
                    
                    

                    if (currentSpeed < speed) {
                        currentSpeed += accelerationMPSS * Time.deltaTime;
                        if (currentSpeed > speed) {
                            currentSpeed = speed; // Ensure we don't exceed the target speed
                        }
                    }

                    transform.Translate(dirToGoal * (currentSpeed * Time.deltaTime / substepsPerTick), Space.World);
                    if (DetectObstacleByRaycast()) break;
                    if (ReachedGoal()) break;
                }

                currentSessionIdleTime = 0;
                cumulativeEmission += currentSpeed * Time.deltaTime * emissionRate;
            }
        }

        private void OnDrawGizmos()
        {
            // Goal
            Gizmos.DrawSphere(goal, 1);

            if (route.Count < 2) return;
            Gizmos.DrawWireCube(new Vector3(route[1].x, 0, route[1].y) + Vector3.up, new Vector3(3, .5f, 3));
        }

        private void OnMouseDown()
        {
            drawPathLine = !drawPathLine;
            lineRenderer.enabled = drawPathLine;
        }

        public bool DetectObstacleByRaycast()
        {
            return raycastPoints.Any(point =>
                Physics.Raycast(point.position, point.forward, inFrontThreshold, carMask));
        }

        public void DrawPath()
        {
            // Draw planned route to destination
            lineRenderer.positionCount = route.Count + 1 + visited.Count;


            if (route.Count < 1) return;

            //Debug.Log(visited);

            for (var i = 0; i < visited.Count; i++)
                lineRenderer.SetPosition(i, new Vector3(visited[i].x, 1f, visited[i].y));
            
            lineRenderer.SetPosition(visited.Count, transform.position);

            for (var i = 0; i < route.Count; i++)
                lineRenderer.SetPosition(visited.Count + i + 1, new Vector3(route[i].x, 1f, route[i].y));
        }

        public void UpdateIntersectionLights()
        {
            // Intersection Lights
            if (currentIntersection != null)
            {
                if (blockedByIntersection)
                    // Stopped
                    trafficLights.State = TrafficLights.LightState.RED;
                // Gizmos.color = Color.red;
                // Gizmos.DrawSphere(transform.position + 2 * Vector3.up, 0.5f);
                else if (currentIntersection.inYellowPhase)
                    // Yellow light moving
                    trafficLights.State = TrafficLights.LightState.YELLOW;
                // Gizmos.color = Color.yellow;
                // Gizmos.DrawSphere(transform.position + 2 * Vector3.up, 0.5f);
                else
                    // Green light moving
                    trafficLights.State = TrafficLights.LightState.GREEN;
                // Gizmos.color = Color.green;
                // Gizmos.DrawSphere(transform.position + 2 * Vector3.up, 0.5f);
            }
            else
            {
                trafficLights.State = TrafficLights.LightState.HIDDEN;
            }
        }

        public void OnReachGoal()
        {

            var rand = UnityEngine.Random.Range(0f, 1f);
            var currentRoad = roadManager.roads[currentRoadId];
            if (rand <= 0.01f && currentRoad.RealEndPos == goal && currentRoad.neighbors.Count > 1) {

                // gives 3 randomly generated next waypoints for the car to travel in, until it receives its new route
                var neighbors = currentRoad.neighbors;
                var nextRoadId = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
                var nextRoad = roadManager.roads[nextRoadId];
                prevGoal = goal;
                goal = nextRoad.RealStartPos;
                speed = nextRoad.speedLimit;
                currentRoadId = nextRoadId;
                route = new List<Vector3>();
                route.Add(new Vector3(goal.x, goal.z, nextRoadId));
                visited.Add(prevGoal);

                Debug.Log($"Car {id} is changing road to {nextRoadId}");

                // add the end of the road to the route
                var endOfRoad = nextRoad.RealEndPos;
                route.Add(new Vector3(endOfRoad.x, endOfRoad.z, nextRoadId));

                // add some more randomly generated turns
                for (var i = 0; i < 3; i++) {
                    neighbors = nextRoad.neighbors;
                    nextRoadId = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
                    nextRoad = roadManager.roads[nextRoadId];
                    route.Add(new Vector3(nextRoad.RealStartPos.x, nextRoad.RealStartPos.z, nextRoadId));
                    endOfRoad = nextRoad.RealEndPos;
                    route.Add(new Vector3(endOfRoad.x, endOfRoad.z, nextRoadId));
                }

                return;
            }

            Vector3 nextNode;

            if (route.Count == 0) {
                deadTime += Time.deltaTime;
                if (deadTime >= 10f) {
                    gameObject.SetActive(false);
                    return;
                }
                tickMark.SetActive(true);
                boxCollider.enabled = false;
                vehicleCustomizer.GhostMode();
                rb.isKinematic = true;
                currentRoadId = -1;
                return;
            }

            if (route.Count == 1) {
                nextNode = route[0];
            } else { 
                nextNode = route[1];
            }

            //currentRoadId = (int)route[0].z;
            goal = new Vector3(nextNode.x, 0, nextNode.y);
            speed = roadManager.roads[(int)nextNode.z].speedLimit;
            currentRoadId = (int)nextNode.z;
            visited.Add(route[0]);
            prevGoal = route[0];
            route.RemoveAt(0);
            // if (route.Count > 0)
            //     currentRoadId = (int)route[0].z; // Set current road ID to next road in advance to avoid vehicles from entering full roads
        }

        public bool ReachedGoal()
        {
            if (Vector3.Distance(transform.position, goal) < tolerance)
            {
                transform.position = goal;
                return true;
            }

            if (route.Count == 1 && Vector3.Distance(transform.position, goal) < destTolerance)
            {
                transform.position = goal;
                return true;
            }

            if (route.Count > 1)
            {
                var nextGoal = new Vector3(route[1].x, 0, route[1].y);
                if (Vector3.Distance(transform.position, nextGoal) <= Vector3.Distance(nextGoal, goal))
                {
                    return true;
                }
            }

            if (visited.Count >= 1) {
                var len = visited.Count - 1;
                var prev = new Vector3(visited[len].x, 0, visited[len].y);
                if (transform.position.x <= Math.Max(goal.x, prev.x) && transform.position.x >= Math.Min(goal.x, prev.x) && transform.position.z <= Math.Max(goal.z, prev.z) && transform.position.z >= Math.Min(goal.z, prev.z)) {
                    var dummy = 1;
                } else {
                    // something went wrong and it isnt in between prev and goal
                    transform.position = goal;
                    return true;
                }
            }

            return false;
        }

        public void HandleInitMessage(VehicleInitMessage msg)
        {
            id = msg.id;
            emissionRate = msg.emissionRate / 1000f;
            useAutoFlow = msg.useAutoFlow;
            if (useAutoFlow) name = "AutoFlow-" + id;
            else name = "Selfish-" + id;
            passengerCount = msg.passengerCount;
            currentRoadId = msg.initRoadId; // May be overridden
        }

        public void HandleUpdateMessage(VehicleUpdateMessage msg)
        {
            route = msg.route;
            if (route.Count == 0)
            {
                goal = transform.position;
            }
            else
            {   
                goal = new Vector3(msg.route[0].x, 0, msg.route[0].y);
                speed = roadManager.roads[(int)msg.route[0].z].speedLimit;
                //currentRoadId = (int)route[0].z;
                transform.LookAt(goal);
                //nextRoadLength = roadManager.roads[(int)msg.route[0].z].Length;
            }

            //running = true;
        }

        public void handleConstantUpdate(VehicleUpdateMessage msg) {     
            if (route.Count > 1)
            {   
                while ((msg.route.Count > 1) && visited.Contains(msg.route[0])) msg.route.RemoveAt(0);
                var first = msg.route[0];
                first = new Vector3(first.x, 0, first.y);

                if (Vector3.Distance(transform.position, first) <= 30f) {
                    Debug.Log($"Car {id} is updating route");
                    route = msg.route;
                }
                
            }

        }

        

        public IEnumerator BeginBypassCollisions()
        {
            // bypassCollisions = true;
            // rb.isKinematic = true;

            yield return new WaitForSeconds(3f);

            // bypassCollisions = false;
            // rb.isKinematic = false;
        }

        public bool IsNextGoalOccupied()
        {
            // Note: Destination could be at the start of a road which could be occupied
            if (route.Count == 0) return false;

            //// Encountered road with zero length (double intersection)
            //if (currentRoadId != -1)
            //    if (roadManager.roads[currentRoadId].IsPointRoad)
            //        return Physics.CheckBox(new Vector3(route[2].x, 0, route[2].y) + Vector3.up, new Vector3(3, .5f, 3), // doesn't work because dupes have been elimiinated
            //            Quaternion.identity, carMask, QueryTriggerInteraction.Ignore);

            //if (useAutoFlow)
            //    // Use a tighter detection box
            //    return Physics.CheckBox(new Vector3(route[1].x, 0, route[1].y) + Vector3.up, new Vector3(1, .5f, 1),
            //        Quaternion.identity, carMask, QueryTriggerInteraction.Ignore);

            //return Physics.CheckBox(new Vector3(route[1].x, 0, route[1].y) + Vector3.up, new Vector3(3, .5f, 3),
            //    Quaternion.identity, carMask, QueryTriggerInteraction.Ignore);

            // Use the same check for everything to maximise consistency
            if (route.Count == 1 || Vector3.Distance(transform.position, new Vector3(prevGoal.x, 0, prevGoal.y)) < tolerance)
            {
                // Vehicle has clipped into goal despite being stopped after triggering intersection's hitbox
                return Physics.CheckBox(new Vector3(route[0].x, 0, route[0].y) + Vector3.up, new Vector3(1, .5f, 1),
                    Quaternion.identity, carMask, QueryTriggerInteraction.Ignore);
            } else
            {
                // Vehicle has not yet reached the goal at the end of the road and is stopped after triggering intersection's hitbox
                return Physics.CheckBox(new Vector3(route[1].x, 0, route[1].y) + Vector3.up, new Vector3(1, .5f, 1),
                    Quaternion.identity, carMask, QueryTriggerInteraction.Ignore);
            }
            
        }
    }
}