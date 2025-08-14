using Assets.Scripts;
using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject crosshair;


    [SerializeField] private float distance = 2f;
    [SerializeField] private float height = 2f;
    
    [SerializeField] private float sideLockedOn = 1f;
    [SerializeField] private float distanceLockedOn = 1.3f;
    [SerializeField] private float heightLockedOn = 1.4f;

    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float rotationSmoothTime = 0.1f;

    [SerializeField] private float mouseX;
    [SerializeField] private float mouseY;

    [SerializeField] private bool lockedOn;

    

    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (player.lockedOnPublic)
        {

            // Pivot rotation to face target
            Vector3 lookDir = player.lockedOnGameObjectPublic.transform.position - playerTransform.position;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero)
            {
                Quaternion lockRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lockRotation, Time.deltaTime * 40f);
            }
            Vector3 offset = transform.rotation * new Vector3(sideLockedOn, heightLockedOn, -distanceLockedOn);

            cameraTransform.position = playerTransform.position + offset;
            cameraTransform.LookAt(player.lockedOnGameObjectPublic.transform.position + Vector3.up * 0.4f);

            // Keeps the camera looking towards target after lockOn mode
            Vector3 angles = transform.eulerAngles;
            mouseX = angles.y;
            mouseY = angles.x;
        }
        else
        {
            mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            mouseY = Mathf.Clamp(mouseY, -35f, 44f); // prevent flipping over

            Vector3 targetRotation = new Vector3(mouseY, mouseX);
            currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

            // Apply rotation to pivot (this object)
            transform.position = playerTransform.position;
            transform.rotation = Quaternion.Euler(currentRotation);

            // Offset the camera
            Vector3 offset = transform.rotation * new Vector3(0, height, -distance);
            cameraTransform.position = transform.position + offset;
            cameraTransform.LookAt(playerTransform.position + Vector3.up * 1.5f);
        }

    }
}
