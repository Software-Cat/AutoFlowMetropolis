using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WebSocketTraffic
{
    public class RoadManager : MonoBehaviour
    {
        public VehicleManager vehicleManager;
        public Dictionary<int, RoadInitMessage> roads = new();

        private void OnDrawGizmos()
        {

            // draws a red wire cube at the start of each road for debugging visibility
            foreach (var (id, road) in roads)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(road.RealStartPos, Vector3.one * 15);
            }                
        }

        public void HandleInitMessage(InitMessage msg)
        {
            // initialise roads
            foreach (var road in msg.roads) {
                roads[road.id] = road;
            }
        }

        public bool IsRoadFull(int roadId, int carCount, int busCount, bool isBus)
        {   

            // check if road is full based on vehicle lengths
            var len = roads[roadId].Length;

            // Penalties
            if (roads[roadId].IsPointRoad)
                // Zero length road (double intersection)
                return carCount != 0;
            var spaceUsed = carCount * 5 + busCount * 10;

            var myLength = isBus ? 10 : 5;

            return spaceUsed + myLength > len;
        }
    }
}