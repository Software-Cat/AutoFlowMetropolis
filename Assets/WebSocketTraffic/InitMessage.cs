using System;
using System.Collections.Generic;

namespace WebSocketTraffic
{
    [Serializable]
    public class InitMessage
    {
        public List<string> tiles;
        public int rowWidth;
        public List<VehicleInitMessage> vehicles;
        public List<RoadInitMessage> roads;
        public List<IntersectionMessage> intersections;
    }
}