using System.Collections.Generic;
using UnityEngine;

namespace WebSocketTraffic
{
    public class IntersectionManager : MonoBehaviour
    {
        public RoadManager roadManager;
        public Dictionary<(int, int), Intersection> intersections = new();

        // update each intersection one by one once the init message is received
        public void HandleInitMessage(InitMessage msg)
        {
            foreach (var currentMsg in msg.intersections)
            {
                var key = ((int)currentMsg.id.y, (int)currentMsg.id.x);

                var current = intersections[key];
                current.manager = this;
                current.useAutoFlow = msg.vehicles[0].useAutoFlow; // TODO: Refactor to real useAutoFlow switch
                current.HandleInitMessage(currentMsg);
            }
        }
    }
}