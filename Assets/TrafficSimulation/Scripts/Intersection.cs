// Traffic Simulation
// //

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TrafficSimulation
{
    public enum IntersectionType
    {
        STOP,
        TRAFFIC_LIGHT
    }

    public class Intersection : MonoBehaviour
    {
        public IntersectionType intersectionType;
        public int id;

        //For stop only
        public List<Segment> prioritySegments;

        //For traffic lights only
        public float lightsDuration = 8;
        public float orangeLightDuration = 2;
        public List<Segment> lightsNbr1;
        public List<Segment> lightsNbr2;

        [HideInInspector] public bool allowUTurn;

        [HideInInspector] public int currentRedLightsGroup = 1;
        private List<GameObject> memVehiclesInIntersection = new();


        private List<GameObject> memVehiclesQueue = new();
        private TrafficSystem trafficSystem;
        private List<GameObject> vehiclesInIntersection;

        private List<GameObject> vehiclesQueue;

        private void Start()
        {
            vehiclesQueue = new List<GameObject>();
            vehiclesInIntersection = new List<GameObject>();
            if (intersectionType == IntersectionType.TRAFFIC_LIGHT)
                InvokeRepeating("SwitchLights", lightsDuration, lightsDuration);
        }

        private void OnTriggerEnter(Collider _other)
        {
            //Check if vehicle is already in the list if yes abort
            //Also abort if we just started the scene (if vehicles inside colliders at start)
            if (IsAlreadyInIntersection(_other.gameObject) || Time.timeSinceLevelLoad < .5f) return;

            if (_other.tag == "AutonomousVehicle" && intersectionType == IntersectionType.STOP)
                TriggerStop(_other.gameObject);
            else if (_other.tag == "AutonomousVehicle" && intersectionType == IntersectionType.TRAFFIC_LIGHT)
                TriggerLight(_other.gameObject);
        }

        private void OnTriggerExit(Collider _other)
        {
            if (_other.tag == "AutonomousVehicle" && intersectionType == IntersectionType.STOP)
                ExitStop(_other.gameObject);
            else if (_other.tag == "AutonomousVehicle" && intersectionType == IntersectionType.TRAFFIC_LIGHT)
                ExitLight(_other.gameObject);
        }

        private void SwitchLights()
        {
            if (currentRedLightsGroup == 1) currentRedLightsGroup = 2;
            else if (currentRedLightsGroup == 2) currentRedLightsGroup = 1;

            //Wait few seconds after light transition before making the other car move (= orange light)
            Invoke("MoveVehiclesQueue", orangeLightDuration);
        }

        private void TriggerStop(GameObject _vehicle)
        {
            var vehicleAI = _vehicle.GetComponent<VehicleAI>();

            //Depending on the waypoint threshold, the car can be either on the target segment or on the past segment
            var vehicleSegment = vehicleAI.GetSegmentVehicleIsIn();

            if (!IsPrioritySegment(vehicleSegment))
            {
                if (vehiclesQueue.Count > 0 || vehiclesInIntersection.Count > 0)
                {
                    vehicleAI.vehicleStatus = Status.STOP;
                    vehiclesQueue.Add(_vehicle);
                }
                else
                {
                    vehiclesInIntersection.Add(_vehicle);
                    vehicleAI.vehicleStatus = Status.SLOW_DOWN;
                }
            }
            else
            {
                vehicleAI.vehicleStatus = Status.SLOW_DOWN;
                vehiclesInIntersection.Add(_vehicle);
            }
        }

        private void ExitStop(GameObject _vehicle)
        {
            _vehicle.GetComponent<VehicleAI>().vehicleStatus = Status.GO;
            vehiclesInIntersection.Remove(_vehicle);
            vehiclesQueue.Remove(_vehicle);

            if (vehiclesQueue.Count > 0 && vehiclesInIntersection.Count == 0)
                vehiclesQueue[0].GetComponent<VehicleAI>().vehicleStatus = Status.GO;
        }

        private void TriggerLight(GameObject _vehicle)
        {
            var vehicleAI = _vehicle.GetComponent<VehicleAI>();
            var vehicleSegment = vehicleAI.GetSegmentVehicleIsIn();

            if (IsRedLightSegment(vehicleSegment))
            {
                vehicleAI.vehicleStatus = Status.STOP;
                vehiclesQueue.Add(_vehicle);
            }
            else
            {
                vehicleAI.vehicleStatus = Status.GO;
            }
        }

        private void ExitLight(GameObject _vehicle)
        {
            _vehicle.GetComponent<VehicleAI>().vehicleStatus = Status.GO;
        }

        private bool IsRedLightSegment(int _vehicleSegment)
        {
            if (currentRedLightsGroup == 1)
            {
                foreach (var segment in lightsNbr1)
                    if (segment.id == _vehicleSegment)
                        return true;
            }
            else
            {
                foreach (var segment in lightsNbr2)
                    if (segment.id == _vehicleSegment)
                        return true;
            }

            return false;
        }

        private void MoveVehiclesQueue()
        {
            //Move all vehicles in queue
            var nVehiclesQueue = new List<GameObject>(vehiclesQueue);
            foreach (var vehicle in vehiclesQueue)
            {
                var vehicleSegment = vehicle.GetComponent<VehicleAI>().GetSegmentVehicleIsIn();
                if (!IsRedLightSegment(vehicleSegment))
                {
                    vehicle.GetComponent<VehicleAI>().vehicleStatus = Status.GO;
                    nVehiclesQueue.Remove(vehicle);
                }
            }

            vehiclesQueue = nVehiclesQueue;
        }

        private bool IsPrioritySegment(int _vehicleSegment)
        {
            foreach (var s in prioritySegments)
                if (_vehicleSegment == s.id)
                    return true;
            return false;
        }

        private bool IsAlreadyInIntersection(GameObject _target)
        {
            foreach (var vehicle in vehiclesInIntersection)
                if (vehicle.GetInstanceID() == _target.GetInstanceID())
                    return true;
            foreach (var vehicle in vehiclesQueue)
                if (vehicle.GetInstanceID() == _target.GetInstanceID())
                    return true;

            return false;
        }

        public void SaveIntersectionStatus()
        {
            memVehiclesQueue = vehiclesQueue;
            memVehiclesInIntersection = vehiclesInIntersection;
        }

        public void ResumeIntersectionStatus()
        {
            foreach (var v in vehiclesInIntersection)
            foreach (var v2 in memVehiclesInIntersection)
                if (v.GetInstanceID() == v2.GetInstanceID())
                {
                    v.GetComponent<VehicleAI>().vehicleStatus = v2.GetComponent<VehicleAI>().vehicleStatus;
                    break;
                }

            foreach (var v in vehiclesQueue)
            foreach (var v2 in memVehiclesQueue)
                if (v.GetInstanceID() == v2.GetInstanceID())
                {
                    v.GetComponent<VehicleAI>().vehicleStatus = v2.GetComponent<VehicleAI>().vehicleStatus;
                    break;
                }
        }

        public void AutofillNodes()
        {
            // Get needed comps
            var col = GetComponent<Collider>();
            var sys = GetComponentInParent<TrafficSystem>();
            var outgoingNodesInInter = new List<Waypoint>();
            var incomingNodesInInter = new List<Waypoint>();

            // See if in intersection
            foreach (var seg in sys.segments.Select((x, i) => new { Value = x, Index = i }))
            foreach (var node in seg.Value.waypoints.Select((x, i) => new { Value = x, Index = i }))
                if (col.bounds.Contains(node.Value.transform.position))
                {
                    if (node.Index == 0)
                        outgoingNodesInInter.Add(node.Value);
                    else
                        incomingNodesInInter.Add(node.Value);
                }

            // Connections
            foreach (var incoming in incomingNodesInInter)
            {
                incoming.segment.nextSegments.Clear();
                foreach (var outgoing in outgoingNodesInInter)
                {
                    if (!allowUTurn && outgoing.segment.originalDefinition == incoming.segment.originalDefinition)
                        continue;
                    incoming.segment.nextSegments.Add(outgoing.segment);
                }
            }

            // Hoisting nodes closer into the intersection to prevent corner cutting
            var allNodes = new List<Waypoint>();
            allNodes.AddRange(outgoingNodesInInter);
            allNodes.AddRange(incomingNodesInInter);
            foreach (var incoming in allNodes)
            {
                var dirToCenter = transform.position - incoming.transform.position;
                if (Mathf.Abs(dirToCenter.x) > Mathf.Abs(dirToCenter.z))
                    incoming.transform.Translate(new Vector3(dirToCenter.x * .5f, 0, 0));
                if (Mathf.Abs(dirToCenter.z) > Mathf.Abs(dirToCenter.x))
                    incoming.transform.Translate(new Vector3(0, 0, dirToCenter.z * .5f));
            }
        }
    }
}
