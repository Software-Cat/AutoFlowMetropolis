using System;
using UnityEngine;
using System.Collections.Generic;

namespace Concurrent
{
    [Serializable]
    public class RoadInitMsg
    {

        public Vector2 startPos;
        public Vector2 endPos;
        public int id;
        public int laneCount;
        public int speedLimit;
        public int capacity;
        public List<VirtualIntMsg> virtualInts;


        public Vector3 RealStartPos => new(startPos.x, 0, startPos.y);
        public Vector3 RealEndPos => new(endPos.x, 0, endPos.y);
        public Vector3 direction => RealEndPos - RealStartPos;
        public float zRot => -Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        public float Length => Vector3.Distance(RealStartPos, RealEndPos);

        public bool IsPointRoad => Length < 0.01f;
    }
}