using System;
using UnityEngine;
using UnityEngine.InputSystem;

//// Ensure we have a Rigidbody component as it is required to calculate the movement for every player.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera Parameters")]
    [SerializeField]
    private float _cameraSensitivity;

    [Header("Movement Parameters")]
    [SerializeField]
    private float _gravity;

    [SerializeField]
    private float _movementSpeed;

    [SerializeField]
    private float _jumpHeight;

    [Header("Player Parameters")]
    [SerializeField]
    private Transform _cameraTransform;

    [SerializeField]
    private Transform _playerTransform;

    [SerializeField]
    private CharacterController _playerController;

    private bool _isGrounded;
    private float _xRotation; // Keep track of the current rotation of the camera and player on the x-axis.
    private float _yRotation; // Keep track of the current rotation of the camera and player on the y-axis.
    private Vector3 _playerVelocity; // Keep track of the current position of the camera and player on the y-axis.
    private PlayerControls _playerControls;

    /// <summary>
    /// Configure the cursor settings and initialize a new instance of PlayerControls when the script is first loaded.
    /// </summary>
    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // Keep the cursor locked to the center of the game view.

        _playerControls = new PlayerControls();
    }

    /// <summary>
    /// Enable the PlayerControls actions and action maps when the component is enabled.
    /// </summary>
    private void OnEnable()
    {
        _playerControls.Enable();

        _playerControls.Player.Jump.performed += HandleJump;
    }

    /// <summary>
    /// Handle the logic for continuous input such as the camera and movement.
    /// </summary>
    private void Update()
    {
        // Check to see if the player is on the ground or not.
        _isGrounded = Physics.Raycast(_playerTransform.position, Vector3.down, 1.25f) && _playerVelocity.y <= 0f;

        HandleLook();
        HandleMovement();
    }

    /// <summary>
    /// Handle the player's camera and character rotation based on the player's input.
    /// </summary>
    private void HandleLook()
    {
        Vector2 userInput = _cameraSensitivity * Time.deltaTime * _playerControls.Player.Look.ReadValue<Vector2>();

        // Since the axes in which we move our input device are opposite in Unity, we must swap them to ensure correct behavior.
        // For example, moving the mouse up and/or down corresponds to side-to-side mouse movement in Unity, so we need to adjust for this.
        _yRotation += userInput.x;
        _xRotation = Mathf.Clamp(_xRotation - userInput.y, -90f, 90f); // Prevent the player from rotating their head backwards.

        _cameraTransform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);

        // Since we have not yet implemented character models, we will only rotate the entire character on the y-axis.
        // This logic may change to display the character looking upwards once a character model is implemented.
        _playerTransform.rotation = Quaternion.Euler(0, _yRotation, 0); 
    }

    /// <summary>
    /// Handle the player's movement on the x-axis and z-axis.
    /// </summary>
    private void HandleMovement()
    {
        // Ensure we always move relative to the direction we are looking at.
        Vector2 userInput = _playerControls.Player.Movement.ReadValue<Vector2>();
        Vector3 moveDirection = _playerTransform.forward * userInput.y + _playerTransform.right * userInput.x;

        _playerVelocity.y += _gravity * Time.deltaTime;

        _playerController.Move(_movementSpeed * Time.deltaTime * moveDirection);
        _playerController.Move(_playerVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Handle the player's input for jumping.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    private void HandleJump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            // Physics equation to calculate the initial jump velocity for reaching a specific height.
            _playerVelocity.y = (float)Math.Sqrt(-2f * _gravity * _jumpHeight);
        }
    }

    /// <summary>
    /// Disable the PlayerControls actions and action maps when the component is disabled.
    /// </summary>
    private void OnDisable()
    {
        _playerControls.Disable();

        _playerControls.Player.Jump.performed -= HandleJump;
    }
}
