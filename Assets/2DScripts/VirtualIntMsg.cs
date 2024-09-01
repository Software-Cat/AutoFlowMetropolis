using System;
using UnityEngine;
using System.Collections.Generic;

namespace Concurrent
{
    [Serializable]

    public class VirtualIntMsg
    {

        public int id;
        public float x;
        public float y;
        public int lane;
        public int direction;
        public float position;

        public List<int> trafficLightOrder;
        public int lightTiming;
        public int road;


        public bool IsEnd => position == 1f;

        public Vector3 RealPosition => new(x, 0f, y);

    }
}