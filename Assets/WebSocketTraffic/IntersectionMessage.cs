using System;
using System.Collections.Generic;
using UnityEngine;

namespace WebSocketTraffic
{
    [Serializable]
    public class IntersectionMessage
    {
        public Vector2 id;
        public List<int> enterRoadIDs;
        public List<int> exitRoadIDs;
        public List<int> pattern;
        public float greenDuration;
    }
}