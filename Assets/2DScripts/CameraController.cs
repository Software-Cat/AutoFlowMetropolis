using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 200f;

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveSpeed = 3f * Camera.main.orthographicSize;

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;

        transform.position += movement;
        float scroll = Input.GetAxis("Mouse ScrollWheel") * 2f;
        if (scroll != 0 && Camera.main.orthographicSize + scroll > 2) Camera.main.orthographicSize += scroll;
    }
}
