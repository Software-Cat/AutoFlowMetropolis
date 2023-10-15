// Traffic Simulation
// //

using System.Collections.Generic;
using UnityEngine;

namespace TrafficSimulation
{
    public class Segment : MonoBehaviour
    {
        public List<Segment> nextSegments;

        [HideInInspector] public int id;
        [HideInInspector] public List<Waypoint> waypoints;
        [HideInInspector] public SegmentDefinition originalDefinition;

        public bool IsOnSegment(Vector3 _p)
        {
            var ts = GetComponentInParent<TrafficSystem>();

            for (var i = 0; i < waypoints.Count - 1; i++)
            {
                var d1 = Vector3.Distance(waypoints[i].transform.position, _p);
                var d2 = Vector3.Distance(waypoints[i + 1].transform.position, _p);
                var d3 = Vector3.Distance(waypoints[i].transform.position, waypoints[i + 1].transform.position);
                var a = d1 + d2 - d3;
                if (a < ts.segDetectThresh && a > -ts.segDetectThresh)
                    return true;
            }

            return false;
        }
    }
}
