// Traffic Simulation
// //


using System.Collections.Generic;
using UnityEngine;

namespace TrafficSimulation
{
    public class TrafficSystem : MonoBehaviour
    {
        public bool hideGuizmos;
        public float segDetectThresh = 0.1f;
        public ArrowDraw arrowDrawType = ArrowDraw.ByLength;
        public int arrowCount = 1;
        public float arrowDistance = 5;
        public float arrowSizeWaypoint = 1;
        public float arrowSizeIntersection = 0.5f;
        public float waypointSize = 0.5f;
        public string[] collisionLayers;

        public List<Segment> segments = new();
        public List<Intersection> intersections = new();
        public Segment curSegment;

        public List<Waypoint> GetAllWaypoints()
        {
            var points = new List<Waypoint>();

            foreach (var segment in segments) points.AddRange(segment.waypoints);

            return points;
        }

        public void SaveTrafficSystem()
        {
            var its = FindObjectsByType<Intersection>(FindObjectsSortMode.None);
            foreach (var it in its)
                it.SaveIntersectionStatus();
        }

        public void ResumeTrafficSystem()
        {
            var its = FindObjectsByType<Intersection>(FindObjectsSortMode.None);
            foreach (var it in its)
                it.ResumeIntersectionStatus();
        }
    }

    public enum ArrowDraw
    {
        FixedCount,
        ByLength,
        Off
    }
}
