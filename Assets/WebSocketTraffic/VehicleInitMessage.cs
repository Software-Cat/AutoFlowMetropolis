using System;
using System.Collections.Generic;
using UnityEngine;

namespace WebSocketTraffic
{
    [Serializable]
    public class VehicleInitMessage
    {
        public int id;
        public int initRoadId;
        public float rotation;
        [SerializeField] private List<float> position;
        public float emissionRate;
        public bool useAutoFlow;
        public int passengerCount;

        public Vector3 Position => new(position[0], 0, position[1]);
    }
}