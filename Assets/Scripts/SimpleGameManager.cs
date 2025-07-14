using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SimpleGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public bool isMultiplayer = false;
    public int maxPlayers = 22; // 11 per team
    
    [Header("Player Setup")]
    public GameObject playerPrefab;
    public Transform[] playerSpawnPoints;
    
    [Header("Camera")]
    public SimpleCameraController cameraController;
    
    [Header("UI")]
    public Text switchMessageText;
    private GameObject switchMessageCanvas;
    
    [Header("Team Management")]
    public int currentPlayerIndex = 0;
    public List<SimplePlayerController> teamPlayers = new List<SimplePlayerController>();
    public bool skipAutoInitialization = false; // Flag to prevent auto-init when using manual setup
    
    [Header("Ball System")]
    public FootballBall footballBall;
    public bool isPrePlay = true;
    public bool autoSwitchToBallCarrier = true;
    
    [Header("Play Selection")]
    public PlaySelectionUI playSelectionUI;
    public bool playSelectionShown = false;
    public bool playSelected = false;
    
    // Switch cooldown system
    private float lastGlobalSwitchTime = -1f;
    private const float globalSwitchCooldown = 0.3f;
    
    // Singleton instance
    public static SimpleGameManager Instance { get; private set; }
    
    // Events for game state changes
    public System.Action<SimplePlayerController> OnPlayerSwitched;
    public System.Action OnGameStarted;
    public System.Action OnPlayStarted;
    
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
    
    void Update()
    {
        // Centralized input handling to prevent double processing
        HandlePlayerSwitchInput();
    }
    
    void HandlePlayerSwitchInput()
    {
        // Check for Tab key input (keyboard) or X button (controller)
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetButtonDown("Fire2") || 
            Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            Debug.Log($"==== CENTRALIZED INPUT DETECTED ====");
            
            // Check global cooldown to prevent rapid switching
            if (Time.time - lastGlobalSwitchTime < globalSwitchCooldown)
            {
                Debug.LogWarning($"INPUT BLOCKED: Global cooldown active. Last input: {lastGlobalSwitchTime}, Current time: {Time.time}, Cooldown: {globalSwitchCooldown}");
                return;
            }
            
            // If in pre-play, handle play selection first
            if (isPrePlay)
            {
                HandlePrePlayInput();
            }
            // If post-play, handle passing or player switching
            else
            {
                HandlePostPlayInput();
            }
        }
    }
    
    void HandlePostPlayInput()
    {
        // Check if current player is QB and can pass
        if (currentPlayerIndex < teamPlayers.Count && teamPlayers[currentPlayerIndex].position == PlayerPosition.QB)
        {
            // Check if the current play is a pass play
            PlayData currentPlay = GetCurrentPlay();
            if (currentPlay != null && currentPlay.playType == PlayType.Pass)
            {
                // Handle passing
                HandlePassInput();
                return;
            }
        }
        
        // If not passing, handle regular player switching
        if (!autoSwitchToBallCarrier && teamPlayers.Count > 1)
        {
            Debug.Log($"Processing switch input - Current player: {teamPlayers[currentPlayerIndex].playerName}");
            SwitchToNextPlayer();
        }
    }
    
    void HandlePassInput()
    {
        if (footballBall == null) return;
        
        // Get input direction for targeting
        Vector3 inputDirection = GetInputDirection();
        
        Debug.Log($"QB attempting pass with input direction: {inputDirection}");
        
        // Pass the ball
        footballBall.PassBall(inputDirection);
        
        lastGlobalSwitchTime = Time.time;
    }
    
    Vector3 GetInputDirection()
    {
        Vector3 inputDirection = Vector3.zero;
        
        // Keyboard input (WASD)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || 
            Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            if (Input.GetKey(KeyCode.W)) vertical += 1f;
            if (Input.GetKey(KeyCode.S)) vertical -= 1f;
            if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontal += 1f;
            
            inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        }
        
        // Controller input (left stick)
        float stickH = Input.GetAxis("Horizontal");
        float stickV = Input.GetAxis("Vertical");
        
        if (Mathf.Abs(stickH) > 0.1f || Mathf.Abs(stickV) > 0.1f)
        {
            inputDirection = new Vector3(stickH, 0f, stickV).normalized;
        }
        
        return inputDirection;
    }
    
    public PlayData GetCurrentPlay()
    {
        // Get the current play from the PlaySelectionUI
        if (playSelectionUI != null)
        {
            return playSelectionUI.GetSelectedPlay();
        }
        
        return null;
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        // Skip auto-initialization if manual setup is being used
        if (skipAutoInitialization)
        {
            Debug.Log("Skipping auto-initialization due to manual setup");
            return;
        }
        
        // Create initial players (two players for testing switching)
        CreateInitialPlayers();
        
        // Set up camera
        SetupCamera();
        
        // Setup ball system
        SetupBallSystem();
        
        // Game is ready
        OnGameStarted?.Invoke();
        
        // Show play selection UI at start
        StartCoroutine(ShowPlaySelectionAtStart());
    }
    
    public void SetupBallSystem()
    {
        // Find or create the ball
        if (footballBall == null)
        {
            footballBall = FindObjectOfType<FootballBall>();
        }
        
        if (footballBall != null)
        {
            // Subscribe to ball events
            footballBall.OnBallCarrierChanged += OnBallCarrierChanged;
            footballBall.OnBallSnapped += OnBallSnapped;
            footballBall.OnBallCaught += OnBallCaught;
        }
        
        // Start in pre-play state
        isPrePlay = true;
        
        Debug.Log("Ball system initialized");
    }
    
    void HandlePrePlayInput()
    {
        // If play selection UI hasn't been shown yet, show it automatically
        if (!playSelectionShown)
        {
            ShowPlaySelection();
            return;
        }
        
        // If play selection is shown but no play selected yet, let PlaySelectionUI handle input
        if (playSelectionShown && !playSelected)
        {
            Debug.Log("Play selection UI is active - waiting for play selection");
            return;
        }
        
        // If play is selected, snap the ball
        if (playSelected)
        {
            SnapBall();
        }
    }
    
    System.Collections.IEnumerator ShowPlaySelectionAtStart()
    {
        // Wait a frame to ensure all systems are initialized
        yield return null;
        
        // Show play selection UI automatically at start
        if (!playSelectionShown)
        {
            ShowPlaySelection();
        }
    }
    
    void ShowPlaySelection()
    {
        if (playSelectionUI == null)
        {
            Debug.LogError("PlaySelectionUI not found! Make sure it's assigned in the inspector or created by SimpleTestSetup.");
            return;
        }
        
        Debug.Log("Showing play selection UI");
        playSelectionUI.ShowPlaySelection();
        playSelectionShown = true;
        
        // Subscribe to play selection event if not already subscribed
        playSelectionUI.OnPlaySelected -= OnPlaySelected;
        playSelectionUI.OnPlaySelected += OnPlaySelected;
    }
    
    void OnPlaySelected(PlayData play)
    {
        Debug.Log($"Play selected in GameManager: {play.playName}");
        playSelected = true;
        
        // Now Tab input will snap the ball
        Debug.Log("Play selected - Tab will now snap the ball");
    }
    
    void SnapBall()
    {
        if (footballBall == null) return;
        
        Debug.Log("Snapping ball...");
        footballBall.SnapBall();
        lastGlobalSwitchTime = Time.time;
    }
    
    void OnBallCarrierChanged(SimplePlayerController newCarrier)
    {
        Debug.Log($"Ball carrier changed to: {newCarrier.playerName}");
        
        // On offense, auto-switch to ball carrier
        if (autoSwitchToBallCarrier && !isPrePlay)
        {
            SwitchToPlayer(newCarrier);
        }
    }
    
    void OnBallSnapped()
    {
        Debug.Log("Ball snapped - enabling player movement");
        isPrePlay = false;
        
        // Enable movement for all players
        foreach (var player in teamPlayers)
        {
            player.EnableMovement();
        }
        
        // Trigger the play started event for play system
        OnPlayStarted?.Invoke();
    }
    
    void OnBallCaught()
    {
        Debug.Log("Ball caught");
        
        // Auto-switch to ball carrier if enabled
        if (autoSwitchToBallCarrier && footballBall != null && footballBall.currentCarrier != null)
        {
            SwitchToPlayer(footballBall.currentCarrier);
        }
    }
    
    void CreateInitialPlayers()
    {
        // Check if players already exist in the scene (only active ones)
        SimplePlayerController[] existingPlayers = FindObjectsOfType<SimplePlayerController>();
        List<SimplePlayerController> activeExistingPlayers = new List<SimplePlayerController>();
        
        foreach (var player in existingPlayers)
        {
            if (player.gameObject.activeInHierarchy)
            {
                activeExistingPlayers.Add(player);
            }
        }
        
        if (activeExistingPlayers.Count > 0)
        {
            // Use existing active players
            for (int i = 0; i < activeExistingPlayers.Count; i++)
            {
                // Assign default positions for existing players
                PlayerPosition defaultPos = i == 0 ? PlayerPosition.QB : PlayerPosition.RB;
                activeExistingPlayers[i].SetPlayerInfo(i, $"Player {i + 1}", defaultPos, i == 0, 0); // Both on team 0 for now
                teamPlayers.Add(activeExistingPlayers[i]);
            }
            Debug.Log($"Using {activeExistingPlayers.Count} existing players in scene");
            return;
        }
        
        if (playerPrefab != null)
        {
            Debug.Log("Creating 2 new players from prefab...");
            
            // Create two players for team switching demonstration
            for (int i = 0; i < 2; i++)
            {
                // Get spawn position
                Vector3 spawnPosition = GetPlayerSpawnPosition(i);
                
                // Instantiate player
                GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                playerObject.name = $"Player {i + 1}";
                playerObject.tag = "Player";
                playerObject.SetActive(true); // Make sure it's active
                
                Debug.Log($"Created player {i + 1} at position {spawnPosition}");
                
                // Set up player controller
                SimplePlayerController playerController = playerObject.GetComponent<SimplePlayerController>();
                if (playerController != null)
                {
                    // First player is controlled, others are not
                    // Team 0 = red indicator, Team 1 = blue indicator (for future multiplayer)
                    // Assign default positions for created players
                    PlayerPosition defaultPos = i == 0 ? PlayerPosition.QB : PlayerPosition.RB;
                    playerController.SetPlayerInfo(i, $"Player {i + 1}", defaultPos, i == 0, 0); // Both on team 0 for now
                    teamPlayers.Add(playerController);
                    
                    Debug.Log($"Player {i + 1} controller setup complete. IsLocal: {i == 0}");
                }
                else
                {
                    Debug.LogError($"Player {i + 1} missing SimplePlayerController component!");
                }
                
                // Add required components if not present
                if (playerObject.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = playerObject.AddComponent<Rigidbody>();
                    rb.mass = 1f;
                    rb.drag = 0f; // No drag since we manually stop the player
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
            }
            
            Debug.Log($"Created {teamPlayers.Count} players total");
        }
        else
        {
            Debug.LogError("No player prefab assigned to GameManager!");
        }
    }
    
    void SetupCamera()
    {
        if (cameraController == null)
        {
            // Find camera controller in scene
            cameraController = FindObjectOfType<SimpleCameraController>();
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
        
        // Default spawn positions (spread players out)
        Vector3 basePosition = new Vector3(0, 1f, 0);
        Vector3 offset = new Vector3(playerIndex * 3f, 0, 0); // Space players 3 units apart
        
        return basePosition + offset;
    }
    
    // Public methods for team/player management
    public void SwitchToNextPlayer()
    {
        if (teamPlayers.Count <= 1) return;
        
        // Check global cooldown to prevent rapid switching
        if (Time.time - lastGlobalSwitchTime < globalSwitchCooldown)
        {
            Debug.LogWarning($"GLOBAL SWITCH BLOCKED: Switch attempted but global cooldown active. Last switch: {lastGlobalSwitchTime}, Current time: {Time.time}, Cooldown: {globalSwitchCooldown}");
            return;
        }
        
        // Record the switch time
        lastGlobalSwitchTime = Time.time;
        
        Debug.Log($"SwitchToNextPlayer: Starting with currentPlayerIndex={currentPlayerIndex}, teamPlayers.Count={teamPlayers.Count}");
        Debug.Log($"Global switch time: {Time.time}");
        Debug.Log($"Switching from player {currentPlayerIndex} ({teamPlayers[currentPlayerIndex].playerName} - {teamPlayers[currentPlayerIndex].position})");
        
        // Disable current player
        if (currentPlayerIndex < teamPlayers.Count)
        {
            teamPlayers[currentPlayerIndex].EnableControl(false);
        }
        
        // Switch to next player
        int oldIndex = currentPlayerIndex;
        currentPlayerIndex = (currentPlayerIndex + 1) % teamPlayers.Count;
        Debug.Log($"Player index changed from {oldIndex} to {currentPlayerIndex}");
        Debug.Log($"New active player: {teamPlayers[currentPlayerIndex].playerName} ({teamPlayers[currentPlayerIndex].position})");
        
        // Enable new player
        teamPlayers[currentPlayerIndex].EnableControl(true);
        
        // Update camera
        if (cameraController != null)
        {
            Vector3 oldPos = cameraController.transform.position;
            Vector3 newPlayerPos = teamPlayers[currentPlayerIndex].transform.position;
            
            cameraController.SetTarget(teamPlayers[currentPlayerIndex].transform);
            cameraController.SnapToTarget(); // Instant snap for testing
            
            Vector3 newCameraPos = cameraController.transform.position;
            
            Debug.Log($"CAMERA SWITCH: From {oldPos} to {newCameraPos}");
            Debug.Log($"Player positions: {teamPlayers[0].name} at {teamPlayers[0].transform.position}, {teamPlayers[1].name} at {teamPlayers[1].transform.position}");
            Debug.Log($"Now following {teamPlayers[currentPlayerIndex].name} at {newPlayerPos}");
        }
        else
        {
            Debug.LogWarning("CameraController is null!");
        }
        
        // Trigger event
        OnPlayerSwitched?.Invoke(teamPlayers[currentPlayerIndex]);
        
        // Show UI message
        ShowSwitchMessage($"Now controlling: {teamPlayers[currentPlayerIndex].playerName} ({teamPlayers[currentPlayerIndex].position})");
        
        // Check final state of all players
        CheckAllPlayersState();
        
        Debug.Log($"Switched to player: {teamPlayers[currentPlayerIndex].playerName}");
    }
    
    [ContextMenu("Manual Switch Players")]
    public void ManualSwitchPlayers()
    {
        Debug.Log("Manual switch triggered from context menu");
        SwitchToNextPlayer();
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
    
    public void SwitchToPlayer(SimplePlayerController player)
    {
        int playerIndex = teamPlayers.IndexOf(player);
        if (playerIndex >= 0)
        {
            SwitchToPlayer(playerIndex);
        }
    }
    
    public SimplePlayerController GetCurrentPlayer()
    {
        if (currentPlayerIndex < teamPlayers.Count)
        {
            return teamPlayers[currentPlayerIndex];
        }
        return null;
    }
    
    public void AddPlayer(Vector3 position, string playerName, int team = 0)
    {
        if (playerPrefab != null && teamPlayers.Count < maxPlayers)
        {
            GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
            playerObject.name = playerName;
            
            SimplePlayerController playerController = playerObject.GetComponent<SimplePlayerController>();
            if (playerController != null)
            {
                // Assign default position for added players
                PlayerPosition defaultPos = PlayerPosition.QB; // Default to QB for manually added players
                playerController.SetPlayerInfo(teamPlayers.Count, playerName, defaultPos, false, team);
                teamPlayers.Add(playerController);
            }
        }
    }
    
    // Public getters for other systems
    public List<SimplePlayerController> GetTeamPlayers()
    {
        return teamPlayers;
    }
    
    public bool IsMultiplayer()
    {
        return isMultiplayer;
    }
    
    // Method to manually complete initialization (for manual setup)
    public void CompleteManualInitialization()
    {
        // Set up camera if not already done
        SetupCamera();
        
        // Create UI if needed
        CreateSwitchMessageUI();
        
        // Trigger game started event
        OnGameStarted?.Invoke();
        
        Debug.Log("Manual initialization complete");
    }
    
    void CreateSwitchMessageUI()
    {
        if (switchMessageCanvas == null)
        {
            // Create canvas
            switchMessageCanvas = new GameObject("Switch Message Canvas");
            Canvas canvas = switchMessageCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            // Add canvas scaler
            CanvasScaler scaler = switchMessageCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster
            switchMessageCanvas.AddComponent<GraphicRaycaster>();
            
            // Create text object
            GameObject textObject = new GameObject("Switch Message Text");
            textObject.transform.SetParent(switchMessageCanvas.transform, false);
            
            switchMessageText = textObject.AddComponent<Text>();
            switchMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            switchMessageText.fontSize = 36;
            switchMessageText.color = Color.white;
            switchMessageText.alignment = TextAnchor.MiddleCenter;
            switchMessageText.text = "";
            
            // Position the text
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.8f);
            textRect.anchorMax = new Vector2(0.5f, 0.8f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(400, 50);
            
            // Add outline for better visibility
            Outline outline = textObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);
            
            // Hide initially
            switchMessageText.gameObject.SetActive(false);
            
            Debug.Log("Switch message UI created");
        }
    }
    
    void ShowSwitchMessage(string message)
    {
        Debug.Log($"ShowSwitchMessage called with: '{message}'");
        
        if (switchMessageText != null)
        {
            switchMessageText.text = message;
            switchMessageText.gameObject.SetActive(true);
            
            Debug.Log($"UI message set to: '{switchMessageText.text}' and activated");
            
            // Hide after 2 seconds
            StartCoroutine(HideSwitchMessageAfterDelay(2f));
        }
        else
        {
            Debug.LogWarning("switchMessageText is null!");
        }
    }
    
    IEnumerator HideSwitchMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (switchMessageText != null)
        {
            switchMessageText.gameObject.SetActive(false);
        }
    }

    void CheckAllPlayersState()
    {
        Debug.Log("=== CHECKING ALL PLAYERS STATE ===");
        for (int i = 0; i < teamPlayers.Count; i++)
        {
            Debug.Log($"Player {i}: {teamPlayers[i].playerName} ({teamPlayers[i].position}), isLocalPlayer: {teamPlayers[i].isLocalPlayer}, Position: {teamPlayers[i].transform.position}");
        }
        Debug.Log($"Current player index: {currentPlayerIndex}");
        if (currentPlayerIndex < teamPlayers.Count)
        {
            Debug.Log($"Currently controlling: {teamPlayers[currentPlayerIndex].playerName} ({teamPlayers[currentPlayerIndex].position})");
        }
        Debug.Log("=== END PLAYERS STATE ===");
    }
} 