using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Tile")]
public class TileScriptableObject : ScriptableObject
{
    public GameObject prfab;

    public TileSocketScriptableObject xPlusSocket;
    public TileSocketScriptableObject xMinusSocket;
    public TileSocketScriptableObject zPlusSocket;
    public TileSocketScriptableObject zMinusSocket;

    public int weight = 1;

    public List<TileSocketScriptableObject> xPlusConnections = new();
    public List<TileSocketScriptableObject> xMinusConnections = new();
    public List<TileSocketScriptableObject> zPlusConnections = new();
    public List<TileSocketScriptableObject> zMinusConnections = new();
}
