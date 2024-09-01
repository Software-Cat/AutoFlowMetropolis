using System;
using UnityEngine;
using System.Collections.Generic;

namespace Concurrent
{
    [Serializable]
    public class VehicleInitMsg
    {

        public int id;
        public float emissionRate;
        public bool useAutoFlow;
        public int passengerCount;

        // 0 conventional, 1 electric, 2 bus
        public int type;
        public VirtualIntMsg position;
        public List<VirtualIntMsg> route;
        public Vector3 RealPosition => new(position.x, 0f, position.y);

    }
}