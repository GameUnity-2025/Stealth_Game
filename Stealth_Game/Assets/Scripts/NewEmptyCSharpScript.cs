using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;         // Player
    public Vector3 offset = new Vector3(0, 1.6f, 0);
    public float sensitivity = 200f; // t?c ?? xoay camera

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        // Nh?n input chu?t
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Gi?i h?n góc quay d?c
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Xoay camera theo chu?t
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Xoay player theo chu?t ngang
        target.Rotate(Vector3.up * mouseX);
    }
}
