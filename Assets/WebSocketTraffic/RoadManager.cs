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
            foreach (var (id, road) in roads)
            {
                //if (IsRoadFull(id))
                //{
                //    Gizmos.color = Color.red;
                //    Gizmos.DrawWireCube(road.RealStartPos, Vector3.one * 15);
                //}
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(road.RealStartPos, Vector3.one * 15);
            }                
        }

        public void HandleInitMessage(InitMessage msg)
        {
            foreach (var road in msg.roads) roads[road.id] = road;
        }

        public bool IsRoadFull(int roadId, int carCount, int busCount)
        {
            var len = roads[roadId].Length;
            //var carCount = vehicleManager.vehicles.Values.Count(v => v.currentRoadId == roadId && v.running);

            // Penalties
            if (roads[roadId].IsPointRoad)
                // Zero length road (double intersection)
                return carCount != 0;
            //if (len < 40f)
            //    // Penalty for short roads as they get full too easily
            //    len -= 5f;
            var spaceUsed = carCount * 5 + busCount * 10;

            return spaceUsed >= len * 0.9f;
        }
    }
}