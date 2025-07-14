using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class SceneSetupGuide : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetupScene = true;
    
    [Header("Prefab Creation")]
    public bool createPlayerPrefab = true;
    public bool setupField = true;
    public bool setupCamera = true;
    
    [Header("Debug")]
    public bool showSetupLogs = true;
    
    void Start()
    {
        if (autoSetupScene)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        LogSetup("Starting scene setup...");
        
        // Setup field
        if (setupField)
        {
            SetupFootballField();
        }
        
        // Setup camera
        if (setupCamera)
        {
            SetupCameraSystem();
        }
        
        // Setup game manager
        SetupGameManager();
        
        // Create player prefab
        if (createPlayerPrefab)
        {
            CreatePlayerPrefab();
        }
        
        LogSetup("Scene setup complete!");
        
        // Show final instructions
        ShowFinalInstructions();
    }
    
    void SetupFootballField()
    {
        LogSetup("Setting up football field...");
        
        // Create field object
        GameObject fieldObject = new GameObject("Football Field");
        fieldObject.transform.position = Vector3.zero;
        
        // Add FootballField component
        FootballField field = fieldObject.AddComponent<FootballField>();
        field.generateOnStart = true;
        field.createColliders = true;
        
        LogSetup("Football field created!");
    }
    
    void SetupCameraSystem()
    {
        LogSetup("Setting up camera system...");
        
        // Find or create main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }
        
        // Add camera controller
        CameraController cameraController = mainCamera.GetComponent<CameraController>();
        if (cameraController == null)
        {
            cameraController = mainCamera.gameObject.AddComponent<CameraController>();
        }
        
        // Set camera position
        mainCamera.transform.position = new Vector3(0, 8, -10);
        mainCamera.transform.rotation = Quaternion.Euler(15, 0, 0);
        
        LogSetup("Camera system setup complete!");
    }
    
    void SetupGameManager()
    {
        LogSetup("Setting up game manager...");
        
        // Create game manager object
        GameObject gameManagerObject = GameObject.Find("Game Manager");
        if (gameManagerObject == null)
        {
            gameManagerObject = new GameObject("Game Manager");
        }
        
        // Add GameManager component
        GameManager gameManager = gameManagerObject.GetComponent<GameManager>();
        if (gameManager == null)
        {
            gameManager = gameManagerObject.AddComponent<GameManager>();
        }
        
        // Connect references
        gameManager.footballField = FindObjectOfType<FootballField>();
        gameManager.cameraController = FindObjectOfType<CameraController>();
        
        LogSetup("Game manager setup complete!");
    }
    
    void CreatePlayerPrefab()
    {
        LogSetup("Creating player prefab...");
        
        // Create player object
        GameObject playerObject = new GameObject("Player");
        playerObject.tag = "Player";
        
        // Add visual representation (capsule)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.transform.parent = playerObject.transform;
        visual.transform.localPosition = new Vector3(0, 1, 0);
        visual.name = "Visual";
        
        // Add collider
        CapsuleCollider collider = playerObject.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.5f;
        collider.center = new Vector3(0, 1f, 0);
        
        // Add rigidbody
        Rigidbody rb = playerObject.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.drag = 2f;
        rb.angularDrag = 5f;
        rb.freezeRotation = true;
        
        // Add PlayerController
        PlayerController playerController = playerObject.AddComponent<PlayerController>();
        
        // Add PlayerInput
        PlayerInput playerInput = playerObject.AddComponent<PlayerInput>();
        
        // Try to load input actions
        try
        {
            InputActionAsset inputActions = Resources.Load<InputActionAsset>("FootballInput");
            if (inputActions != null)
            {
                playerInput.actions = inputActions;
            }
            else
            {
                LogSetup("Warning: FootballInput actions not found in Resources folder. You'll need to assign them manually.");
            }
        }
        catch (System.Exception e)
        {
            LogSetup($"Could not load input actions: {e.Message}");
        }
        
        // Create prefab folder if it doesn't exist
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
        }
        
        // Save as prefab (this would need to be done manually in the editor)
        LogSetup("Player object created! Save it as a prefab in Assets/Prefabs/Player.prefab");
        
        // Connect to game manager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            LogSetup("Don't forget to assign the Player prefab to the Game Manager!");
        }
        
        LogSetup("Player prefab setup complete!");
    }
    
    void ShowFinalInstructions()
    {
        string instructions = @"
🎮 FOOTBALL GAME SETUP COMPLETE! 🎮

📋 Final Setup Steps:
1. Move the Input Actions file to Assets/Resources/FootballInput.inputactions
2. Create a Player prefab from the Player object in Assets/Prefabs/
3. Assign the Player prefab to the Game Manager
4. Test the scene!

🎯 Controls:
- WASD / Left Stick: Move
- Left Shift / Right Trigger: Run
- Tab: Switch players (when you have multiple)

🔧 Next Steps:
- Add more players to test team switching
- Customize player visuals and animations
- Add ball and passing mechanics
- Implement multiplayer networking with Mirror

✅ The scene is now ready for basic gameplay testing!
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
    
    [ContextMenu("Show Setup Instructions")]
    public void ShowSetupInstructions()
    {
        ShowFinalInstructions();
    }
} 