using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WebSocketTraffic
{
    public class InitialSpawner : MonoBehaviour
    {
        public List<DummyTileKey> dummySerializedTileKeys;
        public float scaleFactor = 20f;
        public GameObject vehiclePrefab;
        public InitMessage initMsg;

        public int numOfRows;

        public int itemsPerRow;
        public VehicleManager vehicleManager;
        public IntersectionManager intersectionManager;
        public bool timeToSpawn;
        public RoadManager roadManager;
        public string[,] tileGrid;
        public Dictionary<string, GameObject> tileKeys;

        private void Awake()
        {
            tileKeys = dummySerializedTileKeys.ToDictionary(
                c => c.name,
                c => c.obj);
            vehicleManager = GetComponent<VehicleManager>();
            roadManager = GetComponent<RoadManager>();
        }

        public void Update()
        {
            if (timeToSpawn)
            {
                GenerateTerrain();
                GenerateRoads();
                GenerateIntersections();
                GenerateCars();
                timeToSpawn = false;
            }
        }

        private void GenerateTerrain()
        {
            for (var row = 0; row < numOfRows; row++)
            for (var col = 0; col < itemsPerRow; col++)
            {
                var tileDef = tileGrid[row, col];
                var tile = Instantiate(tileKeys[tileDef], new Vector3(col + .5f, 0, row + .5f) * scaleFactor,
                    Quaternion.identity, transform);

                // Intersection
                var inter = tile.GetComponent<Intersection>();
                if (inter != null)
                {
                    inter.id = (row, col);
                    intersectionManager.intersections[(row, col)] = inter;
                }
            }
        }

        private void GenerateCars()
        {
            foreach (var vehicleMsg in initMsg.vehicles)
            {
                var currentCar = Instantiate(vehiclePrefab,
                    vehicleMsg.Position,
                    Quaternion.Euler(0, vehicleMsg.rotation, 0),
                    transform);
                var vehicleComp = currentCar.GetComponent<Vehicle>();
                vehicleComp.HandleInitMessage(vehicleMsg);
                vehicleComp.roadManager = roadManager;
                vehicleComp.spawn = vehicleComp.transform.position;
                vehicleComp.useAutoFlow = vehicleMsg.useAutoFlow;
                vehicleManager.vehicles[vehicleMsg.id] = vehicleComp;
            }

            vehicleManager.canProcessPendingMessage = true;
        }

        private void GenerateIntersections()
        {
            intersectionManager.HandleInitMessage(initMsg);
        }

        private void GenerateRoads()
        {
            roadManager.HandleInitMessage(initMsg);
        }

        public void HandleInitMessage(InitMessage msg)
        {
            initMsg = msg;
            itemsPerRow = initMsg.rowWidth;
            numOfRows = initMsg.tiles.Count / itemsPerRow;
            tileGrid = new string[numOfRows, itemsPerRow];

            // Fill grid
            var rowInd = 0;
            var colInd = 0;
            foreach (var tile in initMsg.tiles)
            {
                tileGrid[rowInd, colInd] = tile;
                colInd++;
                if (colInd >= initMsg.rowWidth)
                {
                    colInd = 0;
                    rowInd++;
                }
            }
        }

        [Serializable]
        public struct DummyTileKey
        {
            public string name;
            public GameObject obj;
        }
    }
}