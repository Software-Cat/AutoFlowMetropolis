using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveFunctionCollapse : MonoBehaviour
{
    public Vector2 size;
    public TileListScriptableObject allTiles;
    public int maxEntropy;
    public List<Vector2> collapsed = new();

    public int[,] entropy;

    private bool finished;
    public List<TileScriptableObject>[,] possibleTiles;

    private void Reset()
    {
        if (allTiles == null) return;
        NewPossibility();
    }

    private void OnDrawGizmos()
    {
        if (possibleTiles == null) return;

        GUI.color = Color.black;
        for (var x = 0; x < size.x; x++)
        for (var z = 0; z < size.y; z++)
            Handles.Label(new Vector3(x, 0, z), possibleTiles[x, z].Count.ToString());
    }

    public void NewPossibility()
    {
        finished = false;

        collapsed = new List<Vector2>();
        possibleTiles = new List<TileScriptableObject>[(int)size.x, (int)size.y];
        entropy = new int[(int)size.x, (int)size.y];
        maxEntropy = allTiles.tiles.Count;

        // Start with all possibilities
        for (var x = 0; x < size.x; x++)
        for (var z = 0; z < size.y; z++)
        {
            possibleTiles[x, z] = new List<TileScriptableObject>(allTiles.tiles);
            entropy[x, z] = maxEntropy;
        }

        foreach (Transform child in transform) DestroyImmediate(child.gameObject);
    }

    public static T RandomChoice<T>(List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static T RandomChoiceByWeight<T>(IEnumerable<T> sequence, Func<T, float> weightSelector)
    {
        var totalWeight = sequence.Sum(weightSelector);
        // The weight we are after...
        var itemWeightIndex = (float)new System.Random().NextDouble() * totalWeight;
        float currentWeightIndex = 0;

        foreach (var item in from weightedItem in sequence
                 select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
        {
            currentWeightIndex += item.Weight;

            // If we've hit or passed the weight we are after for this item then it's the one we want....
            if (currentWeightIndex >= itemWeightIndex)
                return item.Value;
        }

        return default;
    }

    public List<(Vector2, Vector2)> ValidCardinals(Vector2 v)
    {
        var cardinals = new List<(Vector2, Vector2)>();
        if (v.x + 1 < size.x) cardinals.Add((new Vector2(1, 0), new Vector2(v.x + 1, v.y)));
        if (v.x - 1 >= 0) cardinals.Add((new Vector2(-1, 0), new Vector2(v.x - 1, v.y)));
        if (v.y + 1 < size.y) cardinals.Add((new Vector2(0, 1), new Vector2(v.x, v.y + 1)));
        if (v.y - 1 >= 0) cardinals.Add((new Vector2(0, -1), new Vector2(v.x, v.y - 1)));
        return cardinals;
    }

    public void CollapseAll()
    {
        Reset();
        while (!finished) CollapseOnce();
    }

    public void CollapseOnce()
    {
        // Pick out tiles with lowest entropy
        var lowestEntropyOptions = new List<Vector2>();
        var currentMin = int.MaxValue;
        for (var x = 0; x < size.x; x++)
        for (var z = 0; z < size.y; z++)
        {
            // Do not try picking already collapsed cells
            if (collapsed.Contains(new Vector2(x, z))) continue;

            var currentEntropy = entropy[x, z];
            if (currentEntropy < currentMin)
            {
                lowestEntropyOptions = new List<Vector2> { new(x, z) };
                currentMin = currentEntropy;
            }
            else if (currentEntropy == currentMin)
            {
                lowestEntropyOptions.Add(new Vector2(x, z));
            }
        }

        // If there's nothing left to collapse, we've finished
        if (lowestEntropyOptions.Count == 0)
        {
            finished = true;
            return;
        }

        // Collapse random lowest entropy tile
        var toCollapse = RandomChoice(lowestEntropyOptions);

        // Pick random available option to collapse to
        var collapsingInto =
            RandomChoiceByWeight(possibleTiles[(int)toCollapse.x, (int)toCollapse.y], x => x.weight);
        possibleTiles[(int)toCollapse.x, (int)toCollapse.y] = new List<TileScriptableObject> { collapsingInto };
        entropy[(int)toCollapse.x, (int)toCollapse.y] = 1;
        collapsed.Add(toCollapse);

        // Instantiate collapsed
        Instantiate(collapsingInto.prfab, new Vector3(toCollapse.x, 0, toCollapse.y), Quaternion.identity, transform);

        // Update adjacent cells
        var requiresUpdate = ValidCardinals(toCollapse);
        foreach (var (delta, newPos) in requiresUpdate)
        {
            switch (delta)
            {
                case var v when v == new Vector2(1, 0):
                    possibleTiles[(int)newPos.x, (int)newPos.y] = new List<TileScriptableObject>(
                        from tile in possibleTiles[(int)newPos.x, (int)newPos.y]
                        where collapsingInto.xPlusConnections.Contains(tile.xMinusSocket)
                        select tile);
                    break;
                case var v when v == new Vector2(-1, 0):
                    possibleTiles[(int)newPos.x, (int)newPos.y] = new List<TileScriptableObject>(
                        from tile in possibleTiles[(int)newPos.x, (int)newPos.y]
                        where collapsingInto.xMinusConnections.Contains(tile.xPlusSocket)
                        select tile);
                    break;
                case var v when v == new Vector2(0, 1):
                    possibleTiles[(int)newPos.x, (int)newPos.y] = new List<TileScriptableObject>(
                        from tile in possibleTiles[(int)newPos.x, (int)newPos.y]
                        where collapsingInto.zPlusConnections.Contains(tile.zMinusSocket)
                        select tile);
                    break;
                case var v when v == new Vector2(0, -1):
                    possibleTiles[(int)newPos.x, (int)newPos.y] = new List<TileScriptableObject>(
                        from tile in possibleTiles[(int)newPos.x, (int)newPos.y]
                        where collapsingInto.zMinusConnections.Contains(tile.zPlusSocket)
                        select tile);
                    break;
            }

            entropy[(int)newPos.x, (int)newPos.y] = possibleTiles[(int)newPos.x, (int)newPos.y].Count;
        }
    }
}