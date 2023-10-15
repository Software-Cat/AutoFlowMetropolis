// Traffic Simulation
// //

using UnityEditor;
using UnityEngine;

namespace TrafficSimulation
{
    [CustomEditor(typeof(Intersection))]
    public class IntersectionEditor : Editor
    {
        private Intersection intersection;

        private void OnEnable()
        {
            intersection = target as Intersection;
        }

        public override void OnInspectorGUI()
        {
            intersection.intersectionType =
                (IntersectionType)EditorGUILayout.EnumPopup("Intersection type", intersection.intersectionType);

            EditorGUI.BeginDisabledGroup(intersection.intersectionType != IntersectionType.STOP);

            EditorGUILayout.LabelField("Stop", EditorStyles.boldLabel);
            var sPrioritySegments = serializedObject.FindProperty("prioritySegments");
            EditorGUILayout.PropertyField(sPrioritySegments, new GUIContent("Priority Segments"), true);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(intersection.intersectionType != IntersectionType.TRAFFIC_LIGHT);

            EditorGUILayout.LabelField("Traffic Lights", EditorStyles.boldLabel);
            intersection.lightsDuration =
                EditorGUILayout.FloatField("Light Duration (in s.)", intersection.lightsDuration);
            intersection.orangeLightDuration =
                EditorGUILayout.FloatField("Orange Light Duration (in s.)", intersection.orangeLightDuration);
            var sLightsNbr1 = serializedObject.FindProperty("lightsNbr1");
            var sLightsNbr2 = serializedObject.FindProperty("lightsNbr2");
            EditorGUILayout.PropertyField(sLightsNbr1, new GUIContent("Lights #1 (first to be red)"), true);
            EditorGUILayout.PropertyField(sLightsNbr2, new GUIContent("Lights #2"), true);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Autofill Intersection Nodes"))
            {
                EditorHelper.SetUndoGroup("Autofill Intersection Nodes");
                intersection.AutofillNodes();
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
        }
    }
}
