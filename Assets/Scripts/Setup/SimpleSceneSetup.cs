using UnityEngine;

public class SimpleSceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetupScene = true;
    public bool showSetupLogs = true;
    
    void Start()
    {
        if (autoSetupScene)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Simple Scene")]
    public void SetupScene()
    {
        LogSetup("Starting simple scene setup...");
        
        // Create ground plane
        CreateGround();
        
        // Setup camera
        SetupCamera();
        
        // Setup game manager
        SetupGameManager();
        
        // Create player prefab
        CreatePlayerPrefab();
        
        LogSetup("Simple scene setup complete!");
        
        ShowInstructions();
    }
    
    void CreateGround()
    {
        LogSetup("Creating ground plane...");
        
        // Create a simple ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20, 1, 20); // Make it bigger
        
        // Create a simple green material
        Material groundMaterial = new Material(Shader.Find("Standard"));
        groundMaterial.color = new Color(0.2f, 0.8f, 0.2f); // Green color
        ground.GetComponent<Renderer>().material = groundMaterial;
        
        LogSetup("Ground created!");
    }
    
    void SetupCamera()
    {
        LogSetup("Setting up camera system...");
        
        // Find or create main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            
            // Add AudioListener
            cameraObject.AddComponent<AudioListener>();
        }
        
        // Add simple camera controller
        SimpleCameraController cameraController = mainCamera.GetComponent<SimpleCameraController>();
        if (cameraController == null)
        {
            cameraController = mainCamera.gameObject.AddComponent<SimpleCameraController>();
        }
        
        // Set camera position (lower angle for better gameplay view)
        mainCamera.transform.position = new Vector3(0, 6, -12);
        mainCamera.transform.rotation = Quaternion.Euler(8, 0, 0);
        
        LogSetup("Camera system setup complete!");
    }
    
    void SetupGameManager()
    {
        LogSetup("Setting up game manager...");
        
        // Create game manager object
        GameObject gameManagerObject = GameObject.Find("Simple Game Manager");
        if (gameManagerObject == null)
        {
            gameManagerObject = new GameObject("Simple Game Manager");
        }
        
        // Add SimpleGameManager component
        SimpleGameManager gameManager = gameManagerObject.GetComponent<SimpleGameManager>();
        if (gameManager == null)
        {
            gameManager = gameManagerObject.AddComponent<SimpleGameManager>();
        }
        
        // Connect camera reference
        gameManager.cameraController = FindObjectOfType<SimpleCameraController>();
        
        LogSetup("Game manager setup complete!");
    }
    
    void CreatePlayerPrefab()
    {
        LogSetup("Creating player prefab...");
        
        // Create player prefab object (not in scene initially)
        GameObject playerPrefab = new GameObject("Player Prefab");
        playerPrefab.tag = "Player";
        playerPrefab.SetActive(false); // Keep it inactive as it's just a prefab
        
        // Add visual representation (capsule)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.transform.parent = playerPrefab.transform;
        visual.transform.localPosition = new Vector3(0, 1, 0);
        visual.name = "Visual";
        
        // Make it blue so we can see it
        Material playerMaterial = new Material(Shader.Find("Standard"));
        playerMaterial.color = Color.blue;
        visual.GetComponent<Renderer>().material = playerMaterial;
        
        // Add collider
        CapsuleCollider collider = playerPrefab.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.5f;
        collider.center = new Vector3(0, 1f, 0);
        
        // Add rigidbody
        Rigidbody rb = playerPrefab.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.drag = 0f; // No drag since we manually stop the player
        rb.angularDrag = 5f;
        rb.freezeRotation = true;
        
        // Add SimplePlayerController
        SimplePlayerController playerController = playerPrefab.AddComponent<SimplePlayerController>();
        
        // Add PlayerIndicator
        PlayerIndicator playerIndicator = playerPrefab.AddComponent<PlayerIndicator>();
        
        // Create prefab folder if it doesn't exist
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
        }
        
        // Connect to game manager
        SimpleGameManager gameManager = FindObjectOfType<SimpleGameManager>();
        if (gameManager != null)
        {
            // Assign the prefab to the game manager
            gameManager.playerPrefab = playerPrefab;
            
            LogSetup("Player prefab assigned to Game Manager!");
        }
        
        LogSetup("Player prefab created! GameManager will spawn the actual player.");
    }
    
    void ShowInstructions()
    {
        string instructions = @"
🎮 SIMPLE FOOTBALL GAME READY! 🎮

🚀 How to Test:
1. Press PLAY button in Unity
2. You'll see TWO blue players - one with a red pulsing circle underneath
3. Use WASD keys or PS5 controller left stick to move the active player
4. Hold Left Shift or Square button to run faster
5. Press Tab or Triangle to switch between players
6. The camera will follow the active player automatically

🎯 Controls:
- W/A/S/D: Move player (keyboard)
- Left Stick: Move player (PS5 controller)
- Left Shift: Run (keyboard)
- Square Button: Run (PS5 controller)
- Tab / Triangle: Switch players

✅ What's Working:
- TWO players created automatically for team switching
- Red pulsing circle shows which player you're controlling
- Player movement on flat ground (keyboard + PS5 controller)
- Instant stop when you release movement keys/stick
- Camera following the active player
- Running system
- Team switching system ready for multiplayer
- Full controller support

📝 Next Steps:
1. Test the movement - it should work right away!
2. Test player switching with Tab key
3. Add ball mechanics
4. Create proper football field
5. Add multiplayer support with different teams

🔧 To Add More Players:
- Call SimpleGameManager.Instance.AddPlayer(position, name, team)
- Or use the 'Simple Football > Add Another Player' menu

🎮 Controller Support:
- PS5 controller automatically detected
- Left stick for movement
- Square button for running
- Triangle button for player switching

🛠️ Fixed Issues:
- No more duplicate players created
- Player stops immediately when you release movement input
- Responsive movement with no sliding

🔴 Visual Indicator:
- Red pulsing circle shows which player is being controlled
- Ready for team colors (red/blue) in multiplayer

The game now has 2 players and team switching ready for multiplayer!
        ";
        
        Debug.Log(instructions);
    }
    
    void LogSetup(string message)
    {
        if (showSetupLogs)
        {
            Debug.Log($"[Setup] {message}");
        }
    }
} 