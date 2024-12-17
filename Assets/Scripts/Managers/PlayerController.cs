using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float moveSpeed = 4, runMultiplier = 6, airMultiplier = 0.4f,  rotationSpeed = 1f, accelerationMultiplier = 10,
        jumpSpeed = 1.4f, rotationThreshold = 0.01f, verticalVelocityThreshold = 0.01f, cameraClamp = 90, airDrag = 0.5f, groundDrag = 2;
    private float verticalCameraRotation, horizontalCameraRotation;
    [SerializeField]
    AudioClip grassFootsteps, stoneFootsteps, dirtFootsteps, sandFootsteps, waterFootStep;
    [SerializeField]
    InputActionReference moveAction, runAction, jumpAction, lookAction;
    [SerializeField]
    public GameObject cameraTarget;


    Rigidbody rb;
    Animator animator;
    AudioSource audioSource;

    private bool moving = false,grounded = true;
    private Vector2 input;
    private Vector3 moveDirection,velocity, horizontalVelocity,verticalVelocity;


    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void FixedUpdate()
    {
        //  camera
        RotateCamera();


        input = moveAction.action.ReadValue<Vector2>();
        if (input.x != 0 || input.y != 0)
        {
            Move(runAction.action.IsPressed());
        }
        else if (moving)
            StopMoving();

        if (jumpAction.action.IsPressed())
        {
            Jump();
        }

        velocity = rb.linearVelocity;
        horizontalVelocity.y = velocity.y;
        horizontalVelocity.x = velocity.x;
        horizontalVelocity.z = velocity.z;
        ControlGrounded();
        ControlSpeed();
        ControlDrag();

    }
    void Move(bool run)
    {
        moving = true;
        moveDirection = transform.right * input.x + transform.forward * input.y;
        var force = moveDirection.normalized * moveSpeed * accelerationMultiplier;
        if (run) 
            force *= runMultiplier;
        if (!grounded)
            force *= airMultiplier;
        rb.AddForce(force, ForceMode.Force);
        animator.SetFloat("speed", input.magnitude);
    }
    void StopMoving()
    {
        moving = false;
        moveDirection = Vector3.zero;
        animator.SetFloat("speed", 0);
    }

    void Jump()
    {
        if (!grounded)
            return;
        rb.AddForce(Vector3.up * jumpSpeed * accelerationMultiplier, ForceMode.Impulse);
        animator.SetBool("jump", true);
    }

    // drag makes the movement seem more realistic. But it should not happen while in air
    void ControlDrag()
    {
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = airDrag; 
    }

    void ControlGrounded()
    {
        grounded = Mathf.Abs(velocity.y) < verticalVelocityThreshold;
        animator.SetBool("isGrounded", grounded);
    }
    // to move, we are adding force. The force can make us move faster than we should.
    void ControlSpeed()
    {
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            rb.linearVelocity = horizontalVelocity.normalized * moveSpeed +verticalVelocity;
        }
    }
    // this method will be called from animation trigger each time the player seems to take a step
    // trigger footsteps sound effects based on which tile the player is walking on 
    public void Step()
    {
        VoxelType stepingOnTyoe = VoxelType.Light_Grass;//DetectGroundMaterial();
        switch (stepingOnTyoe)
        {
            case VoxelType.Light_Grass:
                audioSource.PlayOneShot(grassFootsteps);
                break;
            case VoxelType.Light_Rocks:
                audioSource.PlayOneShot(stoneFootsteps);
                break;
            case VoxelType.Water:
                audioSource.PlayOneShot(waterFootStep);
                break;
        }
    }

    private void RotateCamera()
    {
        var lookAtDirection = lookAction.action.ReadValue<Vector2>();
        // if there is an input
        if (lookAtDirection.sqrMagnitude >= rotationThreshold)
        {
            verticalCameraRotation = Mathf.Clamp(verticalCameraRotation + lookAtDirection.y * rotationSpeed, -cameraClamp, cameraClamp);
            horizontalCameraRotation = lookAtDirection.x * rotationSpeed;
            // roatating to the sides of the charater view is on the y axis, and rotating up is on the x axis for some readon
            cameraTarget.transform.localRotation = Quaternion.Euler(verticalCameraRotation, 0.0f, 0.0f);
            transform.Rotate(Vector3.up * horizontalCameraRotation) ;
        }
    }
}
