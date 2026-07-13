using UnityEngine;

public class SimpleCameraMove : MonoBehaviour
{
      [Header("Mouse Settings")]
    public float mouseSensitivity = 150f;

    [Header("Look Limit")]
    public float minXRotation = -80f;
    public float maxXRotation = 80f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private void Start()
    {
        Vector3 startRotation = transform.localEulerAngles;
        xRotation = startRotation.x;
        yRotation = startRotation.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
