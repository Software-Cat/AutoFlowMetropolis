using UnityEngine;

namespace Concurrent {
    public class CameraController : MonoBehaviour
    {
        public float moveSpeed = 200f;

        // is it following a vehicle, and if so, which vehicle is it following?
        public int following = -1;
        public int followingID = -1;

        void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            moveSpeed = 3f * Camera.main.orthographicSize;

            // initialise the road generator and check if vehicles have been spawned
            RoadGenerator roadGenerator = FindObjectOfType<RoadGenerator>();
            bool vehiclesSpawned = roadGenerator.vehiclesSpawned;
            AllRoads roadss = roadGenerator.roadss;

            // if the camera is not following a vehicle, follow the first vehicle spawned
            if (followingID == -1 && vehiclesSpawned == true) {
                followingID = roadss.vehicles[0].id;
            }
            
            // if the camera is following a vehicle, move the camera to the vehicle's position
            // otherwise, move the camera with the arrow keys
            if (following == -1 || followingID == -1) {
                Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
                transform.position += movement;
            } else {
                Vehicle2D vehicle = roadGenerator.vehicles[followingID];
                transform.position = new Vector3(vehicle.transform.position.x, 10, vehicle.transform.position.z);
            }

            // if the space key is pressed, toggle between following a vehicle and not following a vehicle
            if (Input.GetKeyDown(KeyCode.Space)) {
                following = following == -1 ? 0 : -1;
            }
            
            // enable scroll to zoom with higher sensitivity
            float scroll = Input.GetAxis("Mouse ScrollWheel") * 50f;
            if (scroll != 0 && Camera.main.orthographicSize + scroll > 2) Camera.main.orthographicSize += scroll;
        }
    }
}

