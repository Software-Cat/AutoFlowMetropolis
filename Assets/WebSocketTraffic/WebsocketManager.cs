using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;

namespace WebSocketTraffic
{

    [System.Serializable]
    public class Tuple
    {
        public int x;
        public int y;
    }

    [System.Serializable]
    public class Dict
    {   
        public int key;
        public List<Tuple> value;
    }

    [System.Serializable]
    public class Root
    {
        public List<Dict> data;
    }
    public class WebsocketManager : MonoBehaviour
    {
        public bool websocketHasInitialized;
        public InitialSpawner initialSpawner;
        public VehicleManager vehicleManager;
        private WebSocket ws;

        public bool amUpdating;

        // Awake is called on loading
        private void Awake()
        {
            ws = new WebSocket("ws://localhost:8001/");
            ws.OnMessage += (sender, e) =>
                HandleMessage(e.Data);
            ws.Connect();

            // Send information every 5 seconds
            InvokeRepeating("SendInfo", 5.0f, 5.0f);
        }

        

        private void SendInfo()
        {
            var vehicles = vehicleManager.vehicles;
            Dictionary<int, List<float>> carInfo = new Dictionary<int, List<float>>();
            string updatemsg = "{";

            foreach (var pair in vehicles)
            {   
                var vehicle = pair.Value;
                updatemsg += vehicle.id + ":{'Metadata':[";
                updatemsg += vehicle.transform.position.x + "," + vehicle.transform.position.y + "," + vehicle.currentRoadId + "],'Routes':[";
                foreach (var routenode in vehicle.route)
                {
                    updatemsg += "(" + routenode.x + "," + routenode.y + "),";
                }
                updatemsg += "]},";
                //Debug.Log(infor[0] + " " + infor[1]);
            }

            updatemsg += "}";

            amUpdating = true;

            ws.Send(updatemsg);
        }

        private void Reset()
        {
            initialSpawner = GetComponent<InitialSpawner>();
            vehicleManager = GetComponent<VehicleManager>();
        }

        private void OnDestroy()
        {
            ws.Close();
        }

        private void HandleMessage(string jsonMsg)
        {
            if (amUpdating) {
                Root newroutes = JsonUtility.FromJson<Root>(jsonMsg);
                // seems to be 0 length
                foreach (var item in newroutes.data)
                {
                    Debug.Log(item.value[0].x + " " + item.value[0].y + " " + 1);
                }
                Debug.Log("Received: " + jsonMsg);

            } else if (websocketHasInitialized) {
                var updateMsg = JsonUtility.FromJson<UpdateMessage>(jsonMsg);
                vehicleManager.HandleUpdateMessage(updateMsg);
            } else {
                var initMsg = JsonUtility.FromJson<InitMessage>(jsonMsg);
                initialSpawner.HandleInitMessage(initMsg);
                websocketHasInitialized = true;
                initialSpawner.timeToSpawn = true;
            }
        }
    }
}