using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveFunctionCollapse))]
public class WaveFunctionCollapseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var wfc = (WaveFunctionCollapse)target;

        if (GUILayout.Button("Collapse")) wfc.CollapseAll();
        if (GUILayout.Button("New Possibility")) wfc.NewPossibility();
        if (GUILayout.Button("Collapse Once")) wfc.CollapseOnce();
    }
}
