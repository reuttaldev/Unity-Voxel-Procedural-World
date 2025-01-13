using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float moveSpeed = 4, runMultiplier = 6, airMultiplier = 0.4f, accelerationMultiplier = 10, 
        jumpSpeed = 1.4f, airDrag = 0.5f, groundDrag = 2, playerRadius = 0.7f;
    [SerializeField]
    AudioClip grassFootsteps, stoneFootsteps, dirtFootsteps, sandFootsteps, waterFootStep;
    [SerializeField]
    InputActionReference moveAction, runAction, jumpAction;
    [SerializeField]
    public Transform raycastPosition;
    [SerializeField]
    LayerMask GroundLayers;
    AudioSource audioSource;

    Animator animator;
    Rigidbody rb;
    private bool moving = false,grounded = true;
    private Vector2 input;
    private Vector3 moveDirection,velocity, horizontalVelocity,verticalVelocity;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = gameObject.GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        jumpAction.action.performed += Jump;
    }
    private void OnDisable()
    {
        jumpAction.action.performed -= Jump;
    }

    void FixedUpdate()
    {
        input = moveAction.action.ReadValue<Vector2>();
        if (input.x != 0 || input.y != 0)
        {
            Move(runAction.action.IsPressed());
        }
        else if (moving)
            StopMoving();

        velocity = rb.linearVelocity;
        verticalVelocity.y = velocity.y;
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
        // make movement in the air different (slower) compared to normal movement
        if (!grounded)
            force *= airMultiplier;
        rb.AddForce(force, ForceMode.VelocityChange);
        animator.SetFloat("speed", input.magnitude);
    }
    void StopMoving()
    {
        moving = false;
        moveDirection = Vector3.zero;
        animator.SetFloat("speed", 0);
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (!grounded)
            return;
        rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
        animator.SetTrigger("jump");
    }

    // drag makes the movement seem more realistic. But it should not happen while in air
    void ControlDrag()
    {
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = airDrag; 
    }

    // check if player's feet are collidng with anything 
    void ControlGrounded()
    {
        grounded = Physics.CheckSphere(raycastPosition.position, playerRadius,GroundLayers, QueryTriggerInteraction.Ignore);
        animator.SetBool("isGrounded", grounded);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(raycastPosition.position, playerRadius);
    }
#endif
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
            case VoxelType.Dark_Water:
                audioSource.PlayOneShot(waterFootStep);
                break;
        }
    }
}
