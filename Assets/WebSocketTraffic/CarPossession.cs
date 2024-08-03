using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace WebSocketTraffic
{
    public class CarPossession : MonoBehaviour
    {
        [FormerlySerializedAs("playerCamera")] [FormerlySerializedAs("virtualCamera")]
        public CinemachineVirtualCamera followCarCam;

        public Camera minimapCamera;
        public VehicleManager vehicleManager;
        public bool hasPicked; // whether the player has picked a car to follow
        public bool hasRoute; // does this car have a route?
        public Vehicle target;
        public TextMeshProUGUI directionText;
        public float timeSinceLastTurn;
        public TextMeshProUGUI endTimeText; // time when its route is estimated to end
        public TextMeshProUGUI timeLeftText; // time left to reach destination
        public ProgressBar progressBar;
        public int totalNodes;
        public TextMeshProUGUI algoText;

        private void Reset()
        {
            followCarCam = GetComponent<CinemachineVirtualCamera>();
        }

        private void Start()
        {
            progressBar.Percent = 0f;
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

            // Initialize
            if (!hasPicked && vehicleManager.vehicles.Count != 0)
            {

                // Pick a random vehicle to follow & show its path
                target = vehicleManager.vehicles[Random.Range(0, vehicleManager.vehicles.Count)];
                var tr = target.transform;
                followCarCam.Follow = tr;
                followCarCam.LookAt = tr;
                target.drawPathLine = true;
                target.lineRenderer.enabled = true;
                algoText.text = target.useAutoFlow ? "AutoFlow" : "Selfish";
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = true;
                hasPicked = true;
            }

            // if a vehicle is picked and has a route, initialise the estimated end time as 9:00 (placeholder that is changed later in the code)
            if (hasPicked && !hasRoute && target.route.Count > 0)
            {
                totalNodes = target.route.Count;
                endTimeText.text = "9:" + totalNodes.ToString("00");
                hasRoute = true;
            }


            // Tick
            if (hasPicked)
            {
                // Minimap render
                var trans = target.transform;
                minimapCamera.transform.position = trans.position + Vector3.up * 30f;

                // Direction
                if (target.route.Count > 1)
                {
                    var angle = Vector3.SignedAngle(trans.forward, target.goal - trans.position, Vector3.up);
                    var nextGoal = new Vector3(target.route[1].x, 0, target.route[1].z);

                    // detects which direction the car is turning in based on the angle between the car's forward vector and the vector pointing to the next goal
                    if (angle > 5f)
                    {
                        directionText.text = "Turn Right";
                        timeSinceLastTurn = 0f;
                    }
                    else if (angle < -5f)
                    {
                        directionText.text = "Turn Left";
                        timeSinceLastTurn = 0f;
                    }
                    else
                    {
                        timeSinceLastTurn += Time.deltaTime;
                    }

                    if (timeSinceLastTurn > 5f) directionText.text = "Head Straight";
                }

                // Time left
                timeLeftText.text = target.route.Count + " minutes";

                if (totalNodes != 0)
                    // Progress bar
                    progressBar.Percent = 1 - (float)target.route.Count / totalNodes;
            }
        }
    }
}