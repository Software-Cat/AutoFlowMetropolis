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
            accelerationMPSS = 10f;
            currentSpeed = 0f;
            speed = 25f;
            visited = new();
            prevGoal = null;
        }


        private void FixedUpdate() {
            Vector3 curPos = transform.position;
            //Debug.Log("Current position: " + curPos);
            Vector3 nextPos = route[0].RealPosition;
            Vector3 direction = nextPos - curPos;
            //Debug.Log("Direction: " + direction.magnitude);

            if (direction.magnitude < destTolerance) {
                if (route.Count == 1) {
                    Destroy(gameObject);
                    return;
                }
                route.RemoveAt(0);
                prevGoal = route[0];
                nextPos = route[0].RealPosition;
                direction = nextPos - curPos;
                float zRot = -Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(90, 0, zRot);
                Debug.Log("Next goal: " + route[0].id);
            }

            if (currentSpeed < speed) {
                currentSpeed += accelerationMPSS * Time.deltaTime;
            }

            //Debug.Log("Current speed: " + currentSpeed);

            transform.position += direction.normalized * currentSpeed * Time.deltaTime;

            //Debug.Log(Time.deltaTime);

        }


        
    }
}