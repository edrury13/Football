using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Components")]
    private Rigidbody rb;
    private PlayerInput playerInput;
    
    // Input values
    private Vector2 moveInput;
    private bool isRunning;
    
    // For future multiplayer support
    [Header("Player Info")]
    public int playerID;
    public string playerName = "Player";
    public bool isLocalPlayer = true;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        
        // Freeze rotation on X and Z axes (prevent tipping over)
        rb.freezeRotation = true;
    }
    
    void Start()
    {
        // Only enable input for local player (important for multiplayer later)
        if (playerInput != null)
        {
            playerInput.enabled = isLocalPlayer;
        }
    }
    
    void FixedUpdate()
    {
        // Only process movement if this is the local player
        if (isLocalPlayer)
        {
            HandleMovement();
        }
    }
    
    void HandleMovement()
    {
        if (moveInput.magnitude > 0.1f)
        {
            // Calculate movement direction
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
            
            // Move the player
            Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
            
            // Rotate player to face movement direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }
    
    // Input System callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        if (isLocalPlayer)
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }
    
    public void OnRun(InputAction.CallbackContext context)
    {
        if (isLocalPlayer)
        {
            isRunning = context.ReadValueAsButton();
            // Adjust speed based on running state
            moveSpeed = isRunning ? 8f : 5f;
        }
    }
    
    public void OnSwitchPlayer(InputAction.CallbackContext context)
    {
        if (isLocalPlayer && context.performed)
        {
            // Tell the GameManager to switch to the next player
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SwitchToNextPlayer();
            }
        }
    }
    
    // Public methods for team/multiplayer management
    public void SetPlayerInfo(int id, string name, bool isLocal)
    {
        playerID = id;
        playerName = name;
        isLocalPlayer = isLocal;
        
        // Update input system accordingly
        if (playerInput != null)
        {
            playerInput.enabled = isLocalPlayer;
        }
    }
    
    public void EnableControl(bool enable)
    {
        isLocalPlayer = enable;
        if (playerInput != null)
        {
            playerInput.enabled = enable;
        }
    }
} 