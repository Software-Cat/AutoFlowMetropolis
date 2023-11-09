﻿using System.Collections;
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
        public int currentRoadId = -1;
        public float speed = 5f;
        public List<Vector3> route = new();
        public float rotSpeed = 10f;
        public float tolerance = 0.5f;
        public RoadManager roadManager;
        public GameObject tickMark;
        public List<Transform> raycastPoints;
        public float inFrontThreshold = 1f;
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

        private int carMask;

        public List<Vector3> visited = new();

        public Vector3 prevGoal;

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
                return route.Count switch
                {
                    0 => -1,
                    1 => (int)route[0].z,
                    _ => (int)route[1].z
                };
            }
        }

        private void Awake()
        {
            carMask = 1 << LayerMask.NameToLayer("AutonomousVehicle");
        }

        private void Reset()
        {
            boxCollider = GetComponent<BoxCollider>();
            vehicleCustomizer = GetComponent<VehicleCustomizer>();
            rb = GetComponent<Rigidbody>();
            trafficLights = GetComponent<TrafficLights>();
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            // Before route given or after route finished
            if (!running) return;

            // Route just finished
            if (route.Count == 0)
            {
                tickMark.SetActive(true);
                boxCollider.enabled = false;
                vehicleCustomizer.GhostMode();
                running = false;
                rb.isKinematic = true;
                currentRoadId = -1;
                return;
            }

            // Next node
            if (ReachedGoal()) OnReachGoal();

            tripTime += Time.deltaTime * passengerCount;

            // Cosmetics
            UpdateIntersectionLights();
            if (drawPathLine) DrawPath();

            // Directions
            var position = transform.position;
            var dirToGoal = (goal - position).normalized;
            var targetRotation = Quaternion.LookRotation(dirToGoal);

            // Intersection logic (CAN RETURN EARLY HERE AND SKIP THE REST!!!)
            if (blockedByIntersection)
            {   
                idleTime += Time.deltaTime * passengerCount;
                cumulativeEmission += 2 * speed * Time.deltaTime * emissionRate; // idle emission
                return;
            }

            // Is hitting car
            if (!DetectObstacleByRaycast() || bypassCollisions)
            {
                for (var i = 0; i < substepsPerTick; i++)
                {
                    // Face goal
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                        rotSpeed * Time.deltaTime / substepsPerTick);
                    transform.Translate(dirToGoal * (speed * Time.deltaTime / substepsPerTick), Space.World);
                    if (DetectObstacleByRaycast()) break;
                    if (ReachedGoal()) break;
                }

                currentSessionIdleTime = 0;
                cumulativeEmission += speed * Time.deltaTime * emissionRate;
            }
            // else if (useAutoFlow)
            // {
            //     transform.Translate(-transform.forward * (speed * Time.deltaTime * .01f), Space.World);
            //     idleTime += Time.deltaTime;
            // }
            else
            {
                idleTime += Time.deltaTime * passengerCount;
                currentSessionIdleTime += Time.deltaTime;
                cumulativeEmission += 2 * speed * Time.deltaTime * emissionRate; // idle emission
            }

            // Collision bypass (likely cars gridlocked)
            if (currentSessionIdleTime >= 100f) StartCoroutine(BeginBypassCollisions());
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
            lineRenderer.positionCount = route.Count + 1;
            lineRenderer.SetPosition(0, transform.position);

            if (route.Count < 1) return;

            for (var i = 0; i < route.Count; i++)
                lineRenderer.SetPosition(i + 1, new Vector3(route[i].x, 1f, route[i].y));
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

            Vector3 nextNode;

            if (route.Count == 1) {
                nextNode = route[0];
            } else {
                var dx1 = goal.x - prevGoal.x;
                dx1 = dx1 / Math.Abs(dx1);
                var dy1 = goal.z - prevGoal.y;
                dy1 = dy1 / Math.Abs(dy1);

                var dx2 = goal.x - route[1].x;
                dx2 = dx2 / Math.Abs(dx2);
                var dy2 = goal.z - route[1].y;
                dy2 = dy2 / Math.Abs(dy2);

                if (dx1 == dx2 && dy1 == dy2) {
                    // raise error
                    Debug.Log("Error: Vehicle " + id);
                    Debug.Log("prevGoal: " + prevGoal);
                    Debug.Log("goal: " + goal);
                    Debug.Log("route[1]: " + route[1]);
                    route.RemoveAt(0);

                }
                
                nextNode = route[1];
            }

            goal = new Vector3(nextNode.x, 0, nextNode.y);
            speed = roadManager.roads[(int)nextNode.z].speedLimit;
            currentRoadId = (int)nextNode.z;
            visited.Add(route[0]);
            prevGoal = route[0];
            route.RemoveAt(0);
        }

        public bool ReachedGoal()
        {
            return Vector3.Distance(transform.position, goal) < tolerance;
        }

        public void HandleInitMessage(VehicleInitMessage msg)
        {
            id = msg.id;
            emissionRate = msg.emissionRate / 1000f;
            name = "Vehicle-" + id;
            useAutoFlow = msg.useAutoFlow;
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
                currentRoadId = (int)route[0].z;
                transform.LookAt(goal);
            }

            running = true;
        }

        public void handleConstantUpdate(VehicleUpdateMessage msg) {     
            if (route.Count > 1 && useAutoFlow)
            {
                route = msg.route;
                while (visited.Contains(route[0]) && route.Count > 1) route.RemoveAt(0);
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
            if (route.Count < 2) return false;

            // Encountered road with zero length (double intersection)
            if (currentRoadId != -1)
                if (roadManager.roads[currentRoadId].IsPointRoad)
                    return Physics.CheckBox(new Vector3(route[2].x, 0, route[2].y) + Vector3.up, new Vector3(3, .5f, 3),
                        Quaternion.identity, carMask, QueryTriggerInteraction.Ignore);

            if (useAutoFlow)
                // Use a tighter detection box
                return Physics.CheckBox(new Vector3(route[1].x, 0, route[1].y) + Vector3.up, new Vector3(1, .5f, 1),
                    Quaternion.identity, carMask, QueryTriggerInteraction.Ignore);

            return Physics.CheckBox(new Vector3(route[1].x, 0, route[1].y) + Vector3.up, new Vector3(3, .5f, 3),
                Quaternion.identity, carMask, QueryTriggerInteraction.Ignore);
        }
    }
}