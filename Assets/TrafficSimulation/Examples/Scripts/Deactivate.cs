using TrafficSimulation;
using UnityEngine;

public class Deactivate : MonoBehaviour
{
    private bool isActive = true;
    private TrafficSystem ts;

    private GameObject[] vehicles;

    private void Start()
    {
        vehicles = GameObject.FindGameObjectsWithTag("AutonomousVehicle");
        ts = FindFirstObjectByType<TrafficSystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isActive)
            {
                isActive = false;
                ts.SaveTrafficSystem();
                foreach (var vehicle in vehicles) vehicle.SetActive(false);
            }
            else
            {
                isActive = true;

                foreach (var vehicle in vehicles) vehicle.SetActive(true);
                ts.ResumeTrafficSystem();
            }
        }
    }
}
