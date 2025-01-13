using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class CameraRotator : MonoBehaviour
{
    [SerializeField]
    private float mouseSensitivity = 1, clamp = 90, rotationThreshold = 0.01f;
    [SerializeField]
    private Transform target, player;
    [SerializeField]
    private InputActionReference lookAction;

    private float yRotation, xRotation;
    private Vector3 input;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        input = lookAction.action.ReadValue<Vector2>();
    }
    private void LateUpdate()
    {

        if (input.sqrMagnitude > rotationThreshold)
        {
            RotateHorizontally();
            RotateVertically();
        }
    }

    private void RotateHorizontally()
    {
        xRotation = input.x * mouseSensitivity ;
        // roatating to the sides of the charater view is on the y axis, and rotating up is on the x axis for some reason
        player.Rotate(xRotation * Vector3.up);
    }

    private void RotateVertically()
    {
        // not using .Rotate bc we need to clamp
        var verticalAddition = input.y * mouseSensitivity;
        yRotation = Mathf.Clamp(yRotation + verticalAddition, -clamp, clamp);
        target.localRotation = Quaternion.Euler(yRotation, 0.0f, 0.0f);
    }
}
