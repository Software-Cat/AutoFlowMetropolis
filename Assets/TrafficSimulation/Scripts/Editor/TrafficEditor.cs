// Traffic Simulation
// //

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TrafficSimulation
{
    [CustomEditor(typeof(TrafficSystem))]
    public class TrafficEditor : Editor
    {
        private Vector3 lastPoint;
        private Waypoint lastWaypoint;

        //References for moving a waypoint
        private Vector3 startPosition;

        private TrafficSystem wps;

        private void OnEnable()
        {
            wps = target as TrafficSystem;
        }

        private void OnSceneGUI()
        {
            var e = Event.current;
            if (e == null) return;

            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out var hit) && e.type == EventType.MouseDown && e.button == 0)
            {
                //Add a new waypoint on mouseclick + shift
                if (e.shift)
                {
                    if (wps.curSegment == null) return;

                    EditorHelper.BeginUndoGroup("Add Waypoint", wps);
                    AddWaypoint(hit.point);

                    //Close Undo Group
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }

                //Create a segment + add a new waypoint on mouseclick + ctrl
                else if (e.control)
                {
                    EditorHelper.BeginUndoGroup("Add Segment", wps);
                    AddSegment(hit.point);
                    AddWaypoint(hit.point);

                    //Close Undo Group
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }

                //Create an intersection type
                else if (e.alt)
                {
                    EditorHelper.BeginUndoGroup("Add Intersection", wps);
                    AddIntersection(hit.point);

                    //Close Undo Group
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }
            }

            //Set waypoint system as the selected gameobject in hierarchy
            Selection.activeGameObject = wps.gameObject;

            //Handle the selected waypoint
            if (lastWaypoint != null)
            {
                //Uses a endless plain for the ray to hit
                var plane = new Plane(Vector3.up.normalized, lastWaypoint.GetVisualPos());
                plane.Raycast(ray, out var dst);
                var hitPoint = ray.GetPoint(dst);

                //Reset lastPoint if the mouse button is pressed down the first time
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    lastPoint = hitPoint;
                    startPosition = lastWaypoint.transform.position;
                }

                //Move the selected waypoint
                if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    var realDPos = new Vector3(hitPoint.x - lastPoint.x, 0, hitPoint.z - lastPoint.z);

                    lastWaypoint.transform.position += realDPos;
                    lastPoint = hitPoint;
                }

                //Release the selected waypoint
                if (e.type == EventType.MouseUp && e.button == 0)
                {
                    var curPos = lastWaypoint.transform.position;
                    lastWaypoint.transform.position = startPosition;
                    Undo.RegisterFullObjectHierarchyUndo(lastWaypoint, "Move Waypoint");
                    lastWaypoint.transform.position = curPos;
                }

                //Draw a Sphere
                Handles.SphereHandleCap(0, lastWaypoint.GetVisualPos(), Quaternion.identity, wps.waypointSize * 2f,
                    EventType.Repaint);
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                SceneView.RepaintAll();
            }

            //Set the current hovering waypoint
            if (lastWaypoint == null)
                lastWaypoint = wps.GetAllWaypoints()
                    .FirstOrDefault(i => EditorHelper.SphereHit(i.GetVisualPos(), wps.waypointSize, ray));

            //Update the current segment to the currently interacting one
            if (lastWaypoint != null && e.type == EventType.MouseDown)
                wps.curSegment = lastWaypoint.segment;

            //Reset current waypoint
            else if (lastWaypoint != null && e.type == EventType.MouseMove) lastWaypoint = null;
        }

        [MenuItem("Component/Traffic Simulation/Create Traffic Objects")]
        private static void CreateTraffic()
        {
            EditorHelper.SetUndoGroup("Create Traffic Objects");

            var mainGo = EditorHelper.CreateGameObject("Traffic System");
            mainGo.transform.position = Vector3.zero;
            EditorHelper.AddComponent<TrafficSystem>(mainGo);

            var segmentsGo = EditorHelper.CreateGameObject("Segments", mainGo.transform);
            segmentsGo.transform.position = Vector3.zero;

            var intersectionsGo = EditorHelper.CreateGameObject("Intersections", mainGo.transform);
            intersectionsGo.transform.position = Vector3.zero;

            //Close Undo Operation
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            //Register an Undo if changes are made after this call
            Undo.RecordObject(wps, "Traffic Inspector Edit");

            //Draw the Inspector
            TrafficEditorInspector.DrawInspector(wps, serializedObject, out var restructureSystem,
                out var buildFromDef);

            //Rename waypoints if some have been deleted
            if (restructureSystem) RestructureSystem();

            if (buildFromDef) BuildFromDef();

            //Repaint the scene if values have been edited
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            serializedObject.ApplyModifiedProperties();
        }

        private void BuildFromDef()
        {
            // Create segments from segment definition
            var segDefs = FindObjectsByType<SegmentDefinition>(FindObjectsSortMode.None);
            foreach (var segDef in segDefs)
            {
                AddSegment(segDef.dir1Start.transform.position);
                wps.curSegment.originalDefinition = segDef;
                AddWaypoint(segDef.dir1Start.transform.position);
                AddWaypoint(segDef.dir1End.transform.position);
                AddSegment(segDef.dir2Start.transform.position);
                wps.curSegment.originalDefinition = segDef;
                AddWaypoint(segDef.dir2Start.transform.position);
                AddWaypoint(segDef.dir2End.transform.position);
            }

            // Create intersections from intersection definition
            var interDefs = FindObjectsByType<IntersectionDefinition>(FindObjectsSortMode.None);
            foreach (var interDef in interDefs) AddIntersection(interDef.transform.position);

            // Auto-connect segments very close to each other
            var segs = FindObjectsByType<Segment>(FindObjectsSortMode.None);
            foreach (var pair in PermutationsWithoutReplacement(segs, 2))
            {
                var pairList = pair.ToList();

                if (Vector3.Distance(pairList[0].waypoints[1].transform.position,
                        pairList[1].waypoints[0].transform.position) < 0.01f)
                    pairList[0].nextSegments.Add(pairList[1]);
            }

            // Auto connect segments in intersections
            var inters = FindObjectsByType<Intersection>(FindObjectsSortMode.None);
            foreach (var inter in inters) inter.AutofillNodes();
        }

        private static IEnumerable<IEnumerable<T>> CombinationsWithoutRepetition<T>(IEnumerable<T> items, int length)
        {
            var i = 0;
            foreach (var item in items)
            {
                if (length == 1)
                    yield return new[] { item };
                else
                    foreach (var result in CombinationsWithoutRepetition(items.Skip(i + 1), length - 1))
                        yield return new[] { item }.Concat(result);

                ++i;
            }
        }

        private static IEnumerable<IEnumerable<T>>
            PermutationsWithoutReplacement<T>(IEnumerable<T> items, int length)
        {
            if (length == 1) return items.Select(t => new[] { t });

            return PermutationsWithoutReplacement(items, length - 1)
                .SelectMany(t => items.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new[] { t2 }));
        }

        private void AddWaypoint(Vector3 position)
        {
            var go = EditorHelper.CreateGameObject("Waypoint-" + wps.curSegment.waypoints.Count,
                wps.curSegment.transform);
            go.transform.position = position;

            var wp = EditorHelper.AddComponent<Waypoint>(go);
            wp.Refresh(wps.curSegment.waypoints.Count, wps.curSegment);

            //Record changes to the TrafficSystem (string not relevant here)
            Undo.RecordObject(wps.curSegment, "");
            wps.curSegment.waypoints.Add(wp);
        }

        private void AddSegment(Vector3 position)
        {
            var segId = wps.segments.Count;
            var segGo = EditorHelper.CreateGameObject("Segment-" + segId, wps.transform.GetChild(0).transform);
            segGo.transform.position = position;

            wps.curSegment = EditorHelper.AddComponent<Segment>(segGo);
            wps.curSegment.id = segId;
            wps.curSegment.waypoints = new List<Waypoint>();
            wps.curSegment.nextSegments = new List<Segment>();

            //Record changes to the TrafficSystem (string not relevant here)
            Undo.RecordObject(wps, "");
            wps.segments.Add(wps.curSegment);
        }

        private void AddIntersection(Vector3 position)
        {
            var intId = wps.intersections.Count;
            var intGo = EditorHelper.CreateGameObject("Intersection-" + intId, wps.transform.GetChild(1).transform);
            intGo.transform.position = position;

            var bc = EditorHelper.AddComponent<BoxCollider>(intGo);
            bc.isTrigger = true;
            bc.size = new Vector3(25, 15, 25);
            var intersection = EditorHelper.AddComponent<Intersection>(intGo);
            intersection.id = intId;

            //Record changes to the TrafficSystem (string not relevant here)
            Undo.RecordObject(wps, "");
            wps.intersections.Add(intersection);
        }

        private void RestructureSystem()
        {
            //Rename and restructure segments and waypoints
            var nSegments = new List<Segment>();
            var itSeg = 0;
            foreach (Transform tS in wps.transform.GetChild(0).transform)
            {
                var segment = tS.GetComponent<Segment>();
                if (segment != null)
                {
                    var nWaypoints = new List<Waypoint>();
                    segment.id = itSeg;
                    segment.gameObject.name = "Segment-" + itSeg;

                    var itWp = 0;
                    foreach (Transform tW in segment.gameObject.transform)
                    {
                        var waypoint = tW.GetComponent<Waypoint>();
                        if (waypoint != null)
                        {
                            waypoint.Refresh(itWp, segment);
                            nWaypoints.Add(waypoint);
                            itWp++;
                        }
                    }

                    segment.waypoints = nWaypoints;
                    nSegments.Add(segment);
                    itSeg++;
                }
            }

            //Check if next segments still exist
            foreach (var segment in nSegments)
            {
                var nNextSegments = new List<Segment>();
                foreach (var nextSeg in segment.nextSegments)
                    if (nextSeg != null)
                        nNextSegments.Add(nextSeg);
                segment.nextSegments = nNextSegments;
            }

            wps.segments = nSegments;

            //Check intersections
            var nIntersections = new List<Intersection>();
            var itInter = 0;
            foreach (Transform tI in wps.transform.GetChild(1).transform)
            {
                var intersection = tI.GetComponent<Intersection>();
                if (intersection != null)
                {
                    intersection.id = itInter;
                    intersection.gameObject.name = "Intersection-" + itInter;
                    nIntersections.Add(intersection);
                    itInter++;
                }
            }

            wps.intersections = nIntersections;

            //Tell Unity that something changed and the scene has to be saved
            if (!EditorUtility.IsDirty(target)) EditorUtility.SetDirty(target);

            Debug.Log("[Traffic Simulation] Successfully rebuilt the traffic system.");
        }
    }
}
