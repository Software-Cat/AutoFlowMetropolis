using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/TileList")]
public class TileListScriptableObject : ScriptableObject
{
    public List<TileScriptableObject> tiles = new();
}
