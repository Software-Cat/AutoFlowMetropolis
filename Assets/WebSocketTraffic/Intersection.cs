using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WebSocketTraffic
{
    /// <summary>
    ///     Schedules intersection lights on first come first serve basis.
    /// </summary>
    public class Intersection : MonoBehaviour
    {
        public List<int> enterRoadIds;
        public List<int> exitRoadIds;
        public List<Vehicle> queuingVehicles = new();
        public float yellowDuration = 1f; // Edit in prefab inspector
        public float greenDuration = 3f; // Between 3 and 8 seconds
        public bool inYellowPhase;
        public int currentAllowedId = -1;
        public List<int> pattern;
        public int patternIndex;
        public bool isLightControlledIntersection = true;
        public IntersectionManager manager;
        public bool useAutoFlow;
        public (int, int) id;

        private void Start()
        {
            if (!isLightControlledIntersection) return;
            StartCoroutine(SwitchSignal());
            InvokeRepeating(nameof(UpdateWaitingVehicles), 0, 0.2f);
        }

        private void OnDrawGizmosSelected()
        {
            var rm = GetComponentInParent<RoadManager>();
            foreach (var roadId in enterRoadIds)
            {
                var end = rm.roads[roadId].endPos;
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(new Vector3(end.x, 1, end.y), 3);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isLightControlledIntersection) return;

            var v = other.GetComponent<Vehicle>();

            // If accidentally bumped in here or not a vehicle, ignore
            if (v == null || !enterRoadIds.Contains(v.currentRoadId)) return;

            v.currentIntersection = this;
            queuingVehicles.Add(v);
            v.blockedByIntersection = true;
            StartCoroutine(ShortBypassCollisions(v));
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isLightControlledIntersection) return;

            var v = other.GetComponent<Vehicle>();
            if (v.currentIntersection == this) v.currentIntersection = null;
            v.bypassCollisions = false;
        }

        public IEnumerator ShortBypassCollisions(Vehicle v)
        {
            v.bypassCollisions = true;
            yield return new WaitForSeconds(0.8f);
            v.bypassCollisions = false;
        }

        public void HandleInitMessage(IntersectionMessage msg)
        {
            id = ((int)msg.id.x, (int)msg.id.y);
            name = "Intersection-" + id;
            enterRoadIds = msg.enterRoadIDs;
            exitRoadIds = msg.exitRoadIDs;
            pattern = msg.pattern.Select(ind => enterRoadIds[ind])
                .ToList(); // Caution: from here on msg.pattern != pattern
            greenDuration = msg.greenDuration - yellowDuration; // Adjust for yellow light

            if (pattern.Count <= 2)
            {
                isLightControlledIntersection = false;
                return;
            }

            currentAllowedId = pattern[patternIndex];
        }

        public void UpdateWaitingVehicles()
        {
            List<Vehicle> tempCopy = new(queuingVehicles);
            foreach (var v in tempCopy) UpdateWaitingVehicle(v);
        }

        public void UpdateWaitingVehicle(Vehicle vehicle)
        {
            var canEnter = false;

            if (vehicle.route.Count == 0)
            {
                // Vehicle has already reached its goal
                canEnter = false;
            }
            else if (vehicle.route.Count == 1 || Vector3.Distance(vehicle.transform.position, vehicle.prevGoal) < vehicle.tolerance)
            {
                // Vehicle has clipped into goal despite being stopped after triggering intersection's hitbox
                canEnter = vehicle.prevGoal.z == currentAllowedId && !vehicle.IsNextGoalOccupied() &&
                            !manager.roadManager.IsRoadFull((int)vehicle.route[0].z) &&
                            !inYellowPhase;
            } else
            {
                // Vehicle has not yet reached the goal at the end of the road and is stopped after triggering intersection's hitbox
                canEnter = vehicle.currentRoadId == currentAllowedId && !vehicle.IsNextGoalOccupied() &&
                            !manager.roadManager.IsRoadFull((int)vehicle.route[1].z) &&
                            !inYellowPhase;
            }

            //if (manager.roadManager.roads[vehicle.currentRoadId].IsPointRoad)
            //{
            //    if (vehicle.route.Count <= 1)
            //    {
            //        canEnter = vehicle.currentRoadId == currentAllowedId && !vehicle.IsNextGoalOccupied();
            //    }
            //    else
            //    {
            //        canEnter = vehicle.currentRoadId == currentAllowedId && !vehicle.IsNextGoalOccupied() &&
            //                   !manager.roadManager.IsRoadFull(vehicle.NextRoadId);
            //    }
            //}
            //else
            //{
            //    canEnter = (vehicle.currentRoadId == currentAllowedId && !vehicle.IsNextGoalOccupied() &&
            //                !manager.roadManager.IsRoadFull(vehicle.NextRoadId)) ||
            //               !enterRoadIds.Contains(vehicle.currentRoadId);
            //}

            // canEnter = (vehicle.currentRoadId == currentAllowedId &&
            //                 !vehicle.IsNextGoalOccupied()) ||
            //                !enterRoadIds.Contains(vehicle.currentRoadId);
            vehicle.blockedByIntersection = !canEnter;

            if (canEnter)
            {
                queuingVehicles.Remove(vehicle);
                vehicle.currentRoadId = (int)vehicle.route[0].z; // Adding car to the next road as soon as it leaves its previous road
            }
        }

        public IEnumerator SwitchSignal()
        {
            while (true)
            {
                // Wait with green
                //if (useAutoFlow && queuingVehicles.All(v => v.currentRoadId != currentAllowedId))
                //     If no cars in this direction, skip its green light
                //    yield return new WaitForSeconds(0.6f);
                //else
                //    yield return new WaitForSeconds(greenDuration);

                // Wait for traffic light
                yield return new WaitForSeconds(greenDuration);

                // Yellow light
                inYellowPhase = true;
                currentAllowedId = -1; // No valid id
                yield return new WaitForSeconds(yellowDuration);
                inYellowPhase = false;

                // Change to new goal
                patternIndex++;
                if (patternIndex >= pattern.Count) patternIndex = 0;
                currentAllowedId = pattern[patternIndex];
            }
        }
    }
}