using UnityEngine;
using System.Collections.Generic;
using WebSocketSharp;
using System.Collections.Generic;
using System.Collections.Concurrent;
using WebSocketTraffic;
using System;

namespace Concurrent {
    public class RoadGenerator : MonoBehaviour
{
    public GameObject oneLane;
    public GameObject twoLane;
    public GameObject threeLane;

    public GameObject intersection;
    public GameObject bgPrefab;

    private WebSocket ws;



    private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>(); 
    
    void populateBG() {
        GameObject bgSegment = Instantiate(bgPrefab, new Vector3(0, -0.1f, 0), Quaternion.identity);
        bgSegment.transform.rotation = Quaternion.Euler(90, 0, 0);
        bgSegment.transform.localScale = new Vector3(10000, 10000, 10000);
    }

    void spawnRoadSegment(GameObject roadSegment, Vector3 startPosition, Vector3 endPosition) {
        // since each road is 1 unit long, this is how many segments we need
        float length = Vector3.Distance(startPosition, endPosition);
        int lengthInt = (int)length;
        Vector3 direction = (endPosition - startPosition) / lengthInt;
        
        float zRot = -Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        
        for (int i = 0; i < lengthInt; i++) {
            GameObject road = Instantiate(roadSegment, startPosition + direction * i, Quaternion.identity);
            road.transform.rotation = Quaternion.Euler(90, 0, zRot);
        }

    }

    void spawnIntersection(Vector3 position) {
        GameObject inter = Instantiate(intersection, new Vector3(position.x, 1, position.y), Quaternion.identity);
        inter.transform.rotation = Quaternion.Euler(90, 0, 0);
        inter.transform.localScale *= position.z;
    }

    private void Awake() {
        ws = new WebSocket("ws://localhost:8001/");
        ws.OnMessage += (sender, e) =>
            HandleMessage(e.Data);
        ws.Connect();
        populateBG();
    }

    private void OnDestroy()
    {
        ws.Close();
    }

    private void Update ()
    {
        // Work the dispatched actions on the Unity main thread
        while(_actions.Count > 0)
        {
            if(_actions.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }

    void HandleMessage(string message) {
        AllRoads roadss = JsonUtility.FromJson<AllRoads>(message);
        foreach (Vector3 inter in roadss.intersections) {
            _actions.Enqueue(() => spawnIntersection(inter));
        }
        foreach (RoadInitMsg road in roadss.roads) {
            GameObject roadSegment = oneLane;
            if (road.laneCount == 2) {
                roadSegment = twoLane;
            } else if (road.laneCount == 3) {
                roadSegment = threeLane;
            }
            var startPosition = road.RealStartPos;
            var endPosition = road.RealEndPos;
            _actions.Enqueue(() => spawnRoadSegment(roadSegment, startPosition, endPosition));
        }
    }
}

}