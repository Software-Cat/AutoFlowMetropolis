using System;
using System.Collections.Generic;
using UnityEngine;

namespace WebSocketTraffic
{
    [Serializable]
    public class VehicleUpdateMessage
    {
        public int id;
        public List<Vector3> route;
    }
}
