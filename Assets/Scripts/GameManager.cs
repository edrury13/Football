using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public bool isMultiplayer = false;
    public int maxPlayers = 22; // 11 per team
    
    [Header("Player Setup")]
    public GameObject playerPrefab;
    public Transform[] playerSpawnPoints;
    
    [Header("Camera")]
    public CameraController cameraController;
    
    [Header("Field")]
    public FootballField footballField;
    
    [Header("Team Management")]
    public int currentPlayerIndex = 0;
    public List<PlayerController> teamPlayers = new List<PlayerController>();
    
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    // Events for game state changes
    public System.Action<PlayerController> OnPlayerSwitched;
    public System.Action OnGameStarted;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        // Initialize field
        if (footballField != null)
        {
            footballField.GenerateField();
        }
        
        // Create initial player
        CreateInitialPlayer();
        
        // Set up camera
        SetupCamera();
        
        // Game is ready
        OnGameStarted?.Invoke();
    }
    
    void CreateInitialPlayer()
    {
        if (playerPrefab != null)
        {
            // Get spawn position
            Vector3 spawnPosition = GetPlayerSpawnPosition(0);
            
            // Instantiate player
            GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerObject.name = "Player 1";
            playerObject.tag = "Player";
            
            // Set up player controller
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetPlayerInfo(0, "Player 1", true);
                teamPlayers.Add(playerController);
            }
            
            // Add required components if not present
            if (playerObject.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = playerObject.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rb.drag = 2f;
                rb.angularDrag = 5f;
                rb.freezeRotation = true;
            }
            
            if (playerObject.GetComponent<CapsuleCollider>() == null)
            {
                CapsuleCollider collider = playerObject.AddComponent<CapsuleCollider>();
                collider.height = 2f;
                collider.radius = 0.5f;
                collider.center = new Vector3(0, 1f, 0);
            }
            
            if (playerObject.GetComponent<PlayerInput>() == null)
            {
                PlayerInput playerInput = playerObject.AddComponent<PlayerInput>();
                
                // Try to find and assign the input actions
                try
                {
                    InputActionAsset inputActions = Resources.Load<InputActionAsset>("FootballInput");
                    if (inputActions != null)
                    {
                        playerInput.actions = inputActions;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Could not load FootballInput actions: " + e.Message);
                }
            }
        }
    }
    
    void SetupCamera()
    {
        if (cameraController == null)
        {
            // Find camera controller in scene
            cameraController = FindObjectOfType<CameraController>();
        }
        
        if (cameraController != null && teamPlayers.Count > 0)
        {
            cameraController.SetTarget(teamPlayers[currentPlayerIndex].transform);
        }
    }
    
    Vector3 GetPlayerSpawnPosition(int playerIndex)
    {
        // Use spawn points if available
        if (playerSpawnPoints != null && playerIndex < playerSpawnPoints.Length)
        {
            return playerSpawnPoints[playerIndex].position;
        }
        
        // Default spawn position (center of field)
        Vector3 fieldCenter = footballField != null ? footballField.transform.position : Vector3.zero;
        return fieldCenter + new Vector3(0, 1f, 0);
    }
    
    // Public methods for team/player management
    public void SwitchToNextPlayer()
    {
        if (teamPlayers.Count <= 1) return;
        
        // Disable current player
        if (currentPlayerIndex < teamPlayers.Count)
        {
            teamPlayers[currentPlayerIndex].EnableControl(false);
        }
        
        // Switch to next player
        currentPlayerIndex = (currentPlayerIndex + 1) % teamPlayers.Count;
        
        // Enable new player
        teamPlayers[currentPlayerIndex].EnableControl(true);
        
        // Update camera
        if (cameraController != null)
        {
            cameraController.SetTarget(teamPlayers[currentPlayerIndex].transform);
        }
        
        // Trigger event
        OnPlayerSwitched?.Invoke(teamPlayers[currentPlayerIndex]);
        
        Debug.Log($"Switched to player: {teamPlayers[currentPlayerIndex].playerName}");
    }
    
    public void SwitchToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= teamPlayers.Count) return;
        
        // Disable current player
        if (currentPlayerIndex < teamPlayers.Count)
        {
            teamPlayers[currentPlayerIndex].EnableControl(false);
        }
        
        // Switch to specified player
        currentPlayerIndex = playerIndex;
        teamPlayers[currentPlayerIndex].EnableControl(true);
        
        // Update camera
        if (cameraController != null)
        {
            cameraController.SetTarget(teamPlayers[currentPlayerIndex].transform);
        }
        
        // Trigger event
        OnPlayerSwitched?.Invoke(teamPlayers[currentPlayerIndex]);
    }
    
    public PlayerController GetCurrentPlayer()
    {
        if (currentPlayerIndex < teamPlayers.Count)
        {
            return teamPlayers[currentPlayerIndex];
        }
        return null;
    }
    
    public void AddPlayer(Vector3 position, string playerName)
    {
        if (playerPrefab != null && teamPlayers.Count < maxPlayers)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
            playerObject.name = playerName;
            
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetPlayerInfo(teamPlayers.Count, playerName, false);
                teamPlayers.Add(playerController);
            }
        }
    }
    
    // Input handling for team switching (will be called by Input System)
    public void OnSwitchPlayer(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SwitchToNextPlayer();
        }
    }
    
    // Public getters for other systems
    public List<PlayerController> GetTeamPlayers()
    {
        return teamPlayers;
    }
    
    public bool IsMultiplayer()
    {
        return isMultiplayer;
    }
    
    public FootballField GetField()
    {
        return footballField;
    }
} 