using UnityEngine;

// Football player positions
public enum PlayerPosition
{
    QB,   // Quarterback
    RB,   // Running Back
    WR1,  // Wide Receiver 1
    WR2,  // Wide Receiver 2
    TE1,  // Tight End 1
    TE2,  // Tight End 2
    C,    // Center
    LG,   // Left Guard
    RG,   // Right Guard
    LT,   // Left Tackle
    RT    // Right Tackle
}

public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;
    
    [Header("Components")]
    private Rigidbody rb;
    
    // Input values
    private Vector2 moveInput;
    private bool isRunning;
    
    // For future multiplayer support
    [Header("Player Info")]
    public int playerID = 0;
    public string playerName = "Player";
    public PlayerPosition position = PlayerPosition.QB;
    public bool isLocalPlayer = true;
    public int teamID = 0; // 0 = Team A, 1 = Team B (for future multiplayer)
    
    [Header("Visual Indicator")]
    private PlayerIndicator playerIndicator;
    
    [Header("Movement Control")]
    public bool canMove = false; // Start with movement disabled (pre-play)
    public bool isAIControlled = false; // AI takes over during plays
    
    // Check if this position can be controlled by user
    public bool IsUserControllable()
    {
        return position == PlayerPosition.QB || 
               position == PlayerPosition.RB || 
               position == PlayerPosition.WR1 || 
               position == PlayerPosition.WR2 || 
               position == PlayerPosition.TE1 || 
               position == PlayerPosition.TE2;
    }
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Add Rigidbody if not present
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure rigidbody settings
        rb.mass = 1f;
        rb.drag = 0f; // No drag since we manually stop the player
        rb.angularDrag = 5f;
        rb.freezeRotation = true; // Prevent tipping over
        
        // Make sure player starts stationary
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Get or add the player indicator
        playerIndicator = GetComponent<PlayerIndicator>();
        if (playerIndicator == null)
        {
            playerIndicator = gameObject.AddComponent<PlayerIndicator>();
        }
        
        Debug.Log($"Player {gameObject.name} created with canMove={canMove}, indicator: {playerIndicator != null}");
    }
    
    void Update()
    {
        // Only process input if this is the local player
        if (isLocalPlayer)
        {
            HandleInput();
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
    
    void HandleInput()
    {
        // Only handle input if this is the local player, movement is allowed, AND not AI controlled
        if (!isLocalPlayer || !canMove || isAIControlled) 
        {
            // Clear input when movement is disabled or AI controlled
            moveInput = Vector2.zero;
            isRunning = false;
            return;
        }
        
        // Get input from Input Manager (supports both keyboard and controller automatically)
        float horizontal = Input.GetAxis("Horizontal"); // A/D keys OR left stick horizontal
        float vertical = Input.GetAxis("Vertical");     // W/S keys OR left stick vertical
        
        moveInput.x = horizontal;
        moveInput.y = vertical;
        
        // Debug input only when there's actual input
        if (moveInput.magnitude > 0.1f)
        {
            Debug.Log($"{playerName}: Input detected - horizontal: {horizontal}, vertical: {vertical}");
        }
        
        // Check for running input (keyboard or controller)
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Fire3"); // Left Shift or controller button
        
        // Player switching is now handled centrally by GameManager to prevent double processing
    }
    
    void HandleMovement()
    {
        // Check if movement is allowed
        if (!canMove)
        {
            // Stop the player and don't process movement
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }
        
        if (moveInput.magnitude > 0.1f)
        {
            Debug.Log($"{playerName}: Processing movement input: {moveInput}");
            
            // Calculate movement direction
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
            
            // Choose speed based on running state
            float currentSpeed = isRunning ? runSpeed : moveSpeed;
            
            // Move the player
            Vector3 movement = moveDirection * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
            
            // Rotate player to face movement direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Stop the player immediately when no input is detected
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // Keep Y velocity for gravity
        }
    }
    
    // Movement control methods
    public void EnableMovement()
    {
        canMove = true;
        Debug.Log($"{playerName}: Movement enabled (canMove={canMove})");
    }
    
    public void DisableMovement()
    {
        canMove = false;
        // Stop immediately
        if (rb != null)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0); 
        }
        // Clear input
        moveInput = Vector2.zero;
        Debug.Log($"{playerName}: Movement disabled (canMove={canMove})");
    }
    
    // AI control methods
    public void SetAIControlled(bool aiControlled)
    {
        isAIControlled = aiControlled;
        
        if (aiControlled)
        {
            // Clear input when AI takes over
            moveInput = Vector2.zero;
            Debug.Log($"{playerName}: AI control enabled");
        }
        else
        {
            Debug.Log($"{playerName}: AI control disabled - player control restored");
        }
    }
    
    public bool IsAIControlled()
    {
        return isAIControlled;
    }
    
    // Public methods for team/multiplayer management
    public void SetPlayerInfo(int id, string name, PlayerPosition pos, bool isLocal, int team = 0)
    {
        Debug.Log($"SetPlayerInfo called for {name}: ID={id}, position={pos}, isLocal={isLocal}, team={team}");
        
        playerID = id;
        playerName = name;
        position = pos;
        isLocalPlayer = isLocal;
        teamID = team;
        
        // Ensure movement is disabled during pre-play
        canMove = false;
        
        Debug.Log($"SetPlayerInfo result: {playerName} ({position}) - isLocalPlayer={isLocalPlayer}, canMove={canMove}");
        
        // Set indicator color based on team (for future multiplayer)
        if (playerIndicator != null)
        {
            // All players are blue as requested
            Color teamColor = Color.blue;
            playerIndicator.SetColor(teamColor);
            playerIndicator.SetActive(isLocal);
        }
    }
    
    [ContextMenu("Show Player State")]
    public void ShowPlayerState()
    {
        Debug.Log($"=== PLAYER STATE FOR {playerName} ===");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"PlayerID: {playerID}");
        Debug.Log($"Position Type: {position}");
        Debug.Log($"Is User Controllable: {IsUserControllable()}");
        Debug.Log($"isLocalPlayer: {isLocalPlayer}");
        Debug.Log($"TeamID: {teamID}");
        Debug.Log($"Has Indicator: {playerIndicator != null}");
        if (playerIndicator != null)
        {
            Debug.Log($"Indicator Active: {playerIndicator.IsActive}");
        }
        Debug.Log($"=== END PLAYER STATE ===");
    }
    
    // Method to sync switch cooldown time
    public void SetLastSwitchTime(float switchTime)
    {
        // This method is no longer needed as input is centralized
        Debug.Log($"SetLastSwitchTime called for {playerName}: {switchTime}");
    }
    
    public void EnableControl(bool enable)
    {
        Debug.Log($"EnableControl called for {playerName}: {enable} (was: {isLocalPlayer})");
        
        isLocalPlayer = enable;
        
        Debug.Log($"EnableControl result for {playerName}: isLocalPlayer={isLocalPlayer}");
        
        // Ensure PlayerIndicator exists before using it
        if (playerIndicator == null)
        {
            playerIndicator = GetComponent<PlayerIndicator>();
            if (playerIndicator == null)
            {
                playerIndicator = gameObject.AddComponent<PlayerIndicator>();
                Debug.Log($"Created PlayerIndicator for {playerName}");
            }
        }
        
        // Show/hide the indicator based on control state
        if (playerIndicator != null)
        {
            playerIndicator.SetActive(enable);
        }
        else
        {
            Debug.LogError($"PlayerIndicator is still null for {playerName}");
        }
        
        if (enable)
        {
            Debug.Log($"Now controlling: {playerName} (Team {teamID})");
        }
        else
        {
            Debug.Log($"Stopped controlling: {playerName} (Team {teamID})");
        }
    }
} 