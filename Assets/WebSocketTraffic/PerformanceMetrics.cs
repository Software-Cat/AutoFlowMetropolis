﻿using System.Linq;
using TMPro;
using UnityEngine;

namespace WebSocketTraffic
{
    public class PerformanceMetrics : MonoBehaviour
    {
        public VehicleManager vehicleManager;
        public TextMeshProUGUI emissionsText;
        public TextMeshProUGUI tripTimeText;
        public TextMeshProUGUI idleTimeText;

        private void Reset()
        {
            vehicleManager = GetComponent<VehicleManager>();
        }

        private void Update()
        {
            //var cumulEm = vehicleManager.vehicles.Values.Sum(vehicle => (vehicle.cumulativeEmission + ((vehicle.cumulativeEmission / (vehicle.tripTime - vehicle.idleTime) * vehicle.idleTime * 2))));
            var cumulEm = 0f;
            for (var i = 0; i < vehicleManager.vehicles.Count; i++) {
                var vehicle = vehicleManager.vehicles[i];
                var dt = vehicle.tripTime - vehicle.idleTime;
                if (dt > 0) {
                    var extra = (vehicle.cumulativeEmission / dt) * vehicle.idleTime * 2;
                    Debug.Log(vehicle.cumulativeEmission + " " + extra);
                    cumulEm += vehicle.cumulativeEmission + (((vehicle.cumulativeEmission / dt) * vehicle.idleTime * 2));
                } else {
                    cumulEm += vehicle.cumulativeEmission;
                }
            }
            emissionsText.text = "Cumulative Emissions: " + cumulEm + "kg CO2";

            var cumulTime = vehicleManager.vehicles.Values.Sum(vehicle => vehicle.tripTime);
            tripTimeText.text = "Cumulative Trip Time: " + cumulTime + "s";

            var cumulIdleTime = vehicleManager.vehicles.Values.Sum(vehicle => vehicle.idleTime);
            idleTimeText.text = "Cumulative Idle Time: " + cumulIdleTime + "s";
        }
    }
}
