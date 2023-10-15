using UnityEngine;
using WebSocketSharp;

namespace WebSocketTraffic
{
    public class WebsocketManager : MonoBehaviour
    {
        public bool websocketHasInitialized;
        public InitialSpawner initialSpawner;
        public VehicleManager vehicleManager;
        private WebSocket ws;

        // Awake is called on loading
        private void Awake()
        {
            ws = new WebSocket("ws://localhost:8001/");
            ws.OnMessage += (sender, e) =>
                HandleMessage(e.Data);
            ws.Connect();
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
            if (websocketHasInitialized)
            {
                var updateMsg = JsonUtility.FromJson<UpdateMessage>(jsonMsg);
                vehicleManager.HandleUpdateMessage(updateMsg);
            }
            else
            {
                var initMsg = JsonUtility.FromJson<InitMessage>(jsonMsg);
                initialSpawner.HandleInitMessage(initMsg);
                websocketHasInitialized = true;
                initialSpawner.timeToSpawn = true;
            }
        }
    }
}