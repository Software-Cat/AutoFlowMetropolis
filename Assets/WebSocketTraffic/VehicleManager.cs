using System.Collections.Generic;
using UnityEngine;

namespace WebSocketTraffic
{
    public class VehicleManager : MonoBehaviour
    {
        public UpdateMessage pendingUpdateMessage;
        public bool canProcessPendingMessage;
        public Dictionary<int, Vehicle> vehicles = new();

        private void Update()
        {
            if (pendingUpdateMessage != null && canProcessPendingMessage)
                foreach (var update in pendingUpdateMessage.updates)
                {
                    var currentVehicle = vehicles[update.id];
                    currentVehicle.HandleUpdateMessage(update);
                }
        }

        public void HandleUpdateMessage(UpdateMessage msg)
        {
            pendingUpdateMessage = msg;
        }
    }
}
