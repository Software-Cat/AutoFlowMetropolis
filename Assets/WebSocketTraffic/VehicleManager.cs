using System.Collections.Generic;
using UnityEngine;

namespace WebSocketTraffic
{
    public class VehicleManager : MonoBehaviour
    {
        public UpdateMessage pendingUpdateMessage;
        public bool canProcessPendingMessage = true;
        public Dictionary<int, Vehicle> vehicles = new();

        public bool isUpdating = false;

        private void Update()
        {
            if (pendingUpdateMessage != null && canProcessPendingMessage && pendingUpdateMessage.updates != null && pendingUpdateMessage.updates.Count != 0)
            {
                foreach (var update in pendingUpdateMessage.updates)
                {
                    var currentVehicle = vehicles[update.id];

                    if (!isUpdating) {
                        currentVehicle.systemRunning = true;
                        currentVehicle.HandleUpdateMessage(update);
                    } else {
                        currentVehicle.handleConstantUpdate(update);
                        
                    }
                }
                pendingUpdateMessage = null;
            }
            // Simultaneously activate all cars
            foreach (Vehicle vehicle in vehicles.Values)
            {
                if (vehicle.route.Count != 0)
                    
                    vehicle.running = true;
            }
        }

        public void HandleUpdateMessage(UpdateMessage msg)
        {
            pendingUpdateMessage = msg;
            
        }
    }
}
