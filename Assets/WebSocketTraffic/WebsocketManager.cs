using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;

namespace WebSocketTraffic {

    public class WebsocketManager : MonoBehaviour
    {
        public bool websocketHasInitialized = false;
        public InitialSpawner initialSpawner;
        public VehicleManager vehicleManager;
        private WebSocket ws;
        public bool amUpdating = false;

        public float updateInterval = 5.0f;

        // Awake is called on loading
        private void Awake()
        {
            ws = new WebSocket("ws://localhost:8001/");
            ws.OnMessage += (sender, e) =>
                HandleMessage(e.Data);
            ws.Connect();

            // Send information every n seconds
            InvokeRepeating("SendInfo", 10.0f, updateInterval);
        }

        

        private void SendInfo()
        {
            var vehicles = vehicleManager.vehicles;
            string updatemsg = "{";

            foreach (var pair in vehicles)
            {  
                var vehicle = pair.Value;
                if (vehicle.useAutoFlow == false) continue;
                updatemsg += vehicle.id + ":{'Metadata':[";
                updatemsg += vehicle.transform.position.x + "," + vehicle.transform.position.z + "," + vehicle.currentRoadId + "],'Routes':[";
                foreach (var routenode in vehicle.route)
                {
                    updatemsg += "(" + routenode.x + "," + routenode.y + "," + routenode.z + "),";
                }
                updatemsg += "]},";
                //Debug.Log(infor[0] + " " + infor[1]);
            }

            updatemsg += "}";

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
            if (websocketHasInitialized) {
                var updateMsg = JsonUtility.FromJson<UpdateMessage>(jsonMsg);
                if (amUpdating) vehicleManager.isUpdating = true;
                vehicleManager.HandleUpdateMessage(updateMsg);
                amUpdating = true;
                Debug.Log("update");
            } else {
                var initMsg = JsonUtility.FromJson<InitMessage>(jsonMsg);
                updateInterval = (float)initMsg.vehicles[0].id;
                Debug.Log(updateInterval);
                initMsg.vehicles.RemoveAt(0);

                initialSpawner.HandleInitMessage(initMsg);
                websocketHasInitialized = true;
                initialSpawner.timeToSpawn = true;
                Debug.Log("init");
            }
        }
    }
}