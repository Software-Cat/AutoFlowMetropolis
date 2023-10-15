using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileScriptableObject))]
public class TileScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var tile = (TileScriptableObject)target;

        if (GUILayout.Button("Auto Fill"))
        {
            tile.xPlusConnections = new List<TileSocketScriptableObject> { tile.xPlusSocket };
            tile.xMinusConnections = new List<TileSocketScriptableObject> { tile.xMinusSocket };
            tile.zPlusConnections = new List<TileSocketScriptableObject> { tile.zPlusSocket };
            tile.zMinusConnections = new List<TileSocketScriptableObject> { tile.zMinusSocket };
        }

        if (GUILayout.Button("Rotate 90"))
            (tile.xPlusSocket, tile.zMinusSocket, tile.xMinusSocket, tile.zPlusSocket) = (tile.zPlusSocket,
                tile.xPlusSocket, tile.zMinusSocket, tile.xMinusSocket);
    }
}
