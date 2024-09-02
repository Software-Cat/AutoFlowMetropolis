using System;
using UnityEngine;
using System.Collections.Generic;

namespace Concurrent
{

    public class VirtualInt : MonoBehaviour
    {

        public int id;
        public float x;
        public float y;
        public int lane;
        public int direction;
        public float position;

        public List<int> trafficLightOrder;
        public int lightTiming;
        public int activeLight;
        public int lightind = 0;

        public int road;

        public bool IsEnd => position == 1f;

        public Vector3 RealPosition => new(x, 0f, y);

        public float timeElapsed = 0f;

        // initialize the traffic light order and first active light
        private void Awake() {
            if (trafficLightOrder.Count > 0) {
                lightind = 0;
                activeLight = trafficLightOrder[lightind];
            }
        }


        // cycling through the traffic light order
        private void Update() {
            if (trafficLightOrder.Count == 0) {
                return;
            }
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= lightTiming) {
                timeElapsed = 0f;
                lightind = (lightind + 1) % trafficLightOrder.Count;
                activeLight = trafficLightOrder[lightind];
            }
        }

    }
}