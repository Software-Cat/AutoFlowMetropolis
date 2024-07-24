using System;
using System.Collections.Generic;
using UnityEngine;

namespace WebSocketTraffic
{
    [Serializable]
    public class AllRoads
    {   
      
        public List<RoadInitMsg> roads;
        public List<Vector3> intersections;
    }
}
