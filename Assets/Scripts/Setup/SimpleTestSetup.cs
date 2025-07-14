using UnityEngine;
using System.Collections.Generic;

public class SimpleTestSetup : MonoBehaviour
{
    [Header("Test Setup")]
    public bool createPlayersOnStart = true;
    
    // Use the formation system for player positioning
    private FormationData currentFormation;
    private PlayData selectedPlay;
    private PlaySelectionUI playSelectionUI;
    
    void Start()
    {
        if (createPlayersOnStart)
        {
            SetupPlaySelection();
        }
    }
    
    void SetupPlaySelection()
    {
        // Create play selection UI
        GameObject playSelectionObject = new GameObject("Play Selection UI");
        playSelectionUI = playSelectionObject.AddComponent<PlaySelectionUI>();
        
        // Set the reference in the game manager
        if (SimpleGameManager.Instance != null)
        {
            SimpleGameManager.Instance.playSelectionUI = playSelectionUI;
        }
        
        // Subscribe to play selection event
        playSelectionUI.OnPlaySelected += OnPlaySelected;
        
        Debug.Log("Play selection UI created - waiting for player to select a play");
    }
    
    void OnPlaySelected(PlayData play)
    {
        selectedPlay = play;
        currentFormation = play.formation;
        
        Debug.Log($"Play selected: {play.playName} using formation: {currentFormation.formationName}");
        
        // Create the team formation based on selected play
        CreateTeamFormation();
    }
    
    [ContextMenu("Create Test Players")]
    public void CreateTestPlayers()
    {
        Debug.Log("Redirecting to Create Team Formation...");
        CreateTeamFormation();
    }
    
    [ContextMenu("Create Team Formation")]
    public void CreateTeamFormation()
    {
        Debug.Log("Creating full team formation...");
        
        // Use the formation from the selected play (or default if called from context menu)
        if (currentFormation == null)
        {
            currentFormation = FormationBook.GetSingleBackFormation();
            Debug.Log($"Using default formation: {currentFormation.formationName}");
        }
        else
        {
            Debug.Log($"Using selected play formation: {currentFormation.formationName}");
        }
        
        // Start the coroutine for proper setup sequencing
        StartCoroutine(CreateTeamFormationCoroutine());
    }
    
    System.Collections.IEnumerator CreateTeamFormationCoroutine()
    {
        // Clean up existing players first
        SimplePlayerController[] existingPlayers = FindObjectsOfType<SimplePlayerController>();
        foreach (var player in existingPlayers)
        {
            if (player != null)
            {
                DestroyImmediate(player.gameObject);
            }
        }
        
        // Create ground if it doesn't exist
        if (GameObject.Find("Ground") == null)
        {
            CreateGround();
        }
        
        // Create camera if needed
        SetupCamera();
        
        // Create all players in formation
        foreach (var playerPos in currentFormation.playerPositions)
        {
            CreatePlayerAtPosition(playerPos.playerPosition, playerPos.position);
        }
        
        // Wait for all players to be set up
        yield return new WaitForSeconds(0.1f);
        
        // Setup game manager after players are ready
        SetupGameManager();
        
        Debug.Log("Team formation setup complete!");
    }
    
    void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20, 1, 20);
        
        Material groundMaterial = new Material(Shader.Find("Standard"));
        groundMaterial.color = new Color(0.2f, 0.8f, 0.2f);
        ground.GetComponent<Renderer>().material = groundMaterial;
    }
    
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
        }
        
        SimpleCameraController cameraController = mainCamera.GetComponent<SimpleCameraController>();
        if (cameraController == null)
        {
            cameraController = mainCamera.gameObject.AddComponent<SimpleCameraController>();
        }
        
        // Configure the camera controller with proper offset and settings
        cameraController.offset = new Vector3(0, 6, -12);
        cameraController.followSpeed = 5f;
        cameraController.rotationSpeed = 3f;
        cameraController.lookAtTarget = true;
        cameraController.lookOffset = new Vector3(0, 0.5f, 0); // Look slightly above the player
        cameraController.positionSmoothTime = 0.3f;
        cameraController.rotationSmoothTime = 0.3f;
        
        // Set initial position (will be overridden when target is set)
        mainCamera.transform.position = new Vector3(0, 6, -12);
        mainCamera.transform.rotation = Quaternion.Euler(8, 0, 0);
        
        Debug.Log("Camera controller configured with offset: " + cameraController.offset);
    }
    
    void CreatePlayerAtPosition(PlayerPosition position, Vector3 formationPosition)
    {
        Debug.Log($"Creating {position} at position {formationPosition}");
        
        // Create player object
        GameObject playerObject = new GameObject($"{position}");
        playerObject.tag = "Player";
        
        // Position player in formation
        Vector3 worldPosition = formationPosition;
        worldPosition.y = 1f; // Keep players above ground
        playerObject.transform.position = worldPosition;
        
        // Add visual (capsule) - all players blue
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.transform.parent = playerObject.transform;
        visual.transform.localPosition = new Vector3(0, 1, 0);
        visual.name = "Visual";
        
        // All players are blue as requested
        Material playerMaterial = new Material(Shader.Find("Standard"));
        playerMaterial.color = Color.blue;
        visual.GetComponent<Renderer>().material = playerMaterial;
        
        // Name tags disabled for cleaner look
        // CreateNameTag(playerObject, position.ToString());
        
        // Add collider
        CapsuleCollider collider = playerObject.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.5f;
        collider.center = new Vector3(0, 1f, 0);
        
        // Add rigidbody
        Rigidbody rb = playerObject.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.drag = 0f;
        rb.angularDrag = 5f;
        rb.freezeRotation = true;
        
        // Add player indicator FIRST
        PlayerIndicator indicator = playerObject.AddComponent<PlayerIndicator>();
        Debug.Log($"Added PlayerIndicator to {playerObject.name}");
        
        // Add player controller
        SimplePlayerController controller = playerObject.AddComponent<SimplePlayerController>();
        
        // Wait a frame then set up the player
        StartCoroutine(SetupPlayerAtPositionAfterFrame(controller, position));
    }
    
    void CreateNameTag(GameObject player, string name)
    {
        // Create a simple 3D text name tag above the player
        GameObject nameTag = new GameObject("NameTag");
        nameTag.transform.parent = player.transform;
        nameTag.transform.localPosition = new Vector3(0, 3f, 0);
        
        // Create 3D text
        TextMesh textMesh = nameTag.AddComponent<TextMesh>();
        textMesh.text = name;
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        
        // Make text face camera
        nameTag.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        
        // Add outline for better visibility
        MeshRenderer meshRenderer = nameTag.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = Color.white;
        }
    }
    
    void SetupGameManager()
    {
        GameObject gameManagerObject = GameObject.Find("Simple Game Manager");
        if (gameManagerObject == null)
        {
            gameManagerObject = new GameObject("Simple Game Manager");
        }
        
        SimpleGameManager gameManager = gameManagerObject.GetComponent<SimpleGameManager>();
        if (gameManager == null)
        {
            gameManager = gameManagerObject.AddComponent<SimpleGameManager>();
        }
        
        // Skip auto-initialization since we're doing manual setup
        gameManager.skipAutoInitialization = true;
        
        // Connect camera reference
        gameManager.cameraController = FindObjectOfType<SimpleCameraController>();
        
        // Create the ball
        CreateBall();
        
        // Create the play manager
        CreatePlayManager();
        
        // Add only user-controllable players to the game manager
        SimplePlayerController[] allPlayers = FindObjectsOfType<SimplePlayerController>();
        List<SimplePlayerController> userControllablePlayers = new List<SimplePlayerController>();
        
        foreach (var player in allPlayers)
        {
            if (player.IsUserControllable())
            {
                userControllablePlayers.Add(player);
                Debug.Log($"Added user-controllable player: {player.playerName} ({player.position})");
            }
            else
            {
                Debug.Log($"Skipped non-controllable player: {player.playerName} ({player.position})");
            }
        }
        
        // Sort by position priority (QB first, then RB, then receivers)
        userControllablePlayers.Sort((p1, p2) => GetPositionPriority(p1.position).CompareTo(GetPositionPriority(p2.position)));
        
        gameManager.GetTeamPlayers().Clear();
        for (int i = 0; i < userControllablePlayers.Count; i++)
        {
            gameManager.GetTeamPlayers().Add(userControllablePlayers[i]);
            
            // Only the first user-controllable player (QB) should have control initially
            userControllablePlayers[i].EnableControl(i == 0);
            // Start with movement disabled (pre-play)
            userControllablePlayers[i].DisableMovement();
            
            Debug.Log($"Player {i}: {userControllablePlayers[i].playerName} ({userControllablePlayers[i].position}), Control: {i == 0}, Position: {userControllablePlayers[i].transform.position}");
        }
        
        // Set the current player index to 0 (QB)
        gameManager.currentPlayerIndex = 0;
        
        // Make sure the camera is following the active player (QB)
        if (gameManager.cameraController != null && userControllablePlayers.Count > 0)
        {
            gameManager.cameraController.SetTarget(userControllablePlayers[0].transform);
            Debug.Log($"Camera now following {userControllablePlayers[0].playerName} ({userControllablePlayers[0].position})");
        }
        
        // Setup ball system
        gameManager.SetupBallSystem();
        
        // Complete the manual initialization
        gameManager.CompleteManualInitialization();
        
        Debug.Log($"Game manager setup with {userControllablePlayers.Count} user-controllable players out of {allPlayers.Length} total players");
    }
    
    int GetPositionPriority(PlayerPosition pos)
    {
        // Define switching priority: QB first, then RB, then receivers
        switch (pos)
        {
            case PlayerPosition.QB: return 0;
            case PlayerPosition.RB: return 1;
            case PlayerPosition.WR1: return 2;
            case PlayerPosition.WR2: return 3;
            case PlayerPosition.TE1: return 4;
            case PlayerPosition.TE2: return 5;
            default: return 99; // Should not happen for user-controllable players
        }
    }

    System.Collections.IEnumerator SetupPlayerAtPositionAfterFrame(SimplePlayerController controller, PlayerPosition position)
    {
        yield return null; // Wait one frame
        
        // Set player info with position
        controller.SetPlayerInfo(
            (int)position,                    // Use enum value as ID
            position.ToString(),              // Use position as name
            position,                         // Set position
            false,                           // Start with no control - GameManager will handle this
            0                                // Team 0
        );
        
        Debug.Log($"{position} setup complete. Control will be managed by GameManager. User controllable: {controller.IsUserControllable()}");
    }
    
    void CreateBall()
    {
        // Check if ball already exists
        if (GameObject.Find("Football") != null)
        {
            Debug.Log("Ball already exists in scene");
            return;
        }
        
        Debug.Log("Creating football ball...");
        
        // Create ball object
        GameObject ballObject = new GameObject("Football");
        ballObject.transform.position = new Vector3(0, 1f, 0); // Start at center line
        
        // Add the ball controller
        FootballBall ballController = ballObject.AddComponent<FootballBall>();
        
        Debug.Log($"Football created at position {ballObject.transform.position}");
        
        // Make sure it's visible in the scene
        ballObject.tag = "Ball";
        ballObject.layer = 0; // Default layer
    }
    
    void CreatePlayManager()
    {
        // Check if play manager already exists
        if (GameObject.Find("Play Manager") != null)
        {
            Debug.Log("Play Manager already exists in scene");
            return;
        }
        
        Debug.Log("Creating Play Manager...");
        
        // Create play manager object
        GameObject playManagerObject = new GameObject("Play Manager");
        PlayManager playManager = playManagerObject.AddComponent<PlayManager>();
        
        Debug.Log("Play Manager created successfully");
    }
    
    public PlayData GetSelectedPlay()
    {
        return selectedPlay;
    }
} 