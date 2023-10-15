using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WebSocketTraffic
{
    public class LandPlotRandomizer : MonoBehaviour
    {
        private static readonly List<float> RotationOptions = new()
        {
            0, 90, 180, 270
        };

        public List<LandPlotDescriptor> prefabs;


        private void Start()
        {
            var choice = prefabs[Random.Range(0, prefabs.Count)];
            var obj = Instantiate(choice.prefab, transform);
            transform.localScale = choice.scale;
            transform.Rotate(0, RotationOptions[Random.Range(0, 4)], 0);
        }

        [Serializable]
        public struct LandPlotDescriptor
        {
            public GameObject prefab;
            public Vector3 scale;
        }
    }
}
