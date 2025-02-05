using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float fastSpeed = 15f;
    public float rotationSpeed = 2f;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private Camera cam;
    public static bool canRotate = true;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleZoom();
    }

    private void HandleMouseLook()
    {
        if (!canRotate)
            return;

        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        rotationY += mouseX;

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float moveZ = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow
        float moveY = 0f;

        if (Input.GetKey(KeyCode.Q)) moveY = 1f;  // Move Up
        if (Input.GetKey(KeyCode.E)) moveY = -1f; // Move Down

        Vector3 moveDirection = (transform.forward * moveZ) + (transform.right * moveX) + (Vector3.up * moveY);
        float speed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : moveSpeed;

        transform.position += moveDirection * speed * Time.deltaTime;
    }


    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.fieldOfView -= scroll * zoomSpeed;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);
    }

    private void SelectObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            MaterialSelector materialSelector = hit.collider.GetComponent<MaterialSelector>();
            if (materialSelector != null)
            {
                // materialSelector.ShowColorDropdown();
            }
        }
    }
}
