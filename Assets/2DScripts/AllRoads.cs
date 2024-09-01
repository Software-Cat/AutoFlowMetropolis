using System;
using System.Collections.Generic;
using UnityEngine;

namespace Concurrent
{
    [Serializable]
    public class AllRoads
    {   
      
        public List<RoadInitMsg> roads;
        public List<VehicleInitMsg> vehicles;
    }
}
