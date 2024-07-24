using System;
using UnityEngine;
using System.Collections.Generic;

namespace WebSocketTraffic
{
    [Serializable]
    public class RoadInitMsg
    {

        public Vector2 startPos;
        public Vector2 endPos;

        public int laneCount;

        public Vector3 RealStartPos => new(startPos.x, 0, startPos.y);
        public Vector3 RealEndPos => new(endPos.x, 0, endPos.y);
        public float Length => Vector3.Distance(RealStartPos, RealEndPos);

        public bool IsPointRoad => Length < 0.01f;
    }
}