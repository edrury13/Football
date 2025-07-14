#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SimpleEditorSetup : MonoBehaviour
{
    [MenuItem("Simple Football/Setup Testing Scene")]
    static void SetupSimpleScene()
    {
        // Create setup object
        GameObject setupObject = new GameObject("Simple Scene Setup");
        SimpleSceneSetup setupScript = setupObject.AddComponent<SimpleSceneSetup>();
        
        // Run the setup
        setupScript.SetupScene();
        
        // Select the setup object in the hierarchy
        Selection.activeGameObject = setupObject;
        
        Debug.Log("Simple football scene setup complete! Press Play to test.");
    }
    
    [MenuItem("Football Game/Setup/Create Team Formation")]
    public static void CreateTeamFormation()
    {
        SimpleTestSetup testSetup = FindObjectOfType<SimpleTestSetup>();
        if (testSetup == null)
        {
            GameObject setupObject = new GameObject("Simple Test Setup");
            testSetup = setupObject.AddComponent<SimpleTestSetup>();
        }
        
        testSetup.CreateTeamFormation();
    }
    
    [MenuItem("Football Game/Setup/Test Setup (Manual)")]
    public static void TestSetupManual()
    {
        // Redirect to team formation
        CreateTeamFormation();
    }
    
    [MenuItem("Simple Football/Add Another Player")]
    static void AddAnotherPlayer()
    {
        SimpleGameManager gameManager = FindObjectOfType<SimpleGameManager>();
        if (gameManager != null)
        {
            int playerCount = gameManager.GetTeamPlayers().Count;
            Vector3 spawnPosition = new Vector3(playerCount * 3f, 1, 0);
            string playerName = $"Player {playerCount + 1}";
            int team = playerCount % 2; // Alternate between teams for demo
            gameManager.AddPlayer(spawnPosition, playerName, team);
            Debug.Log($"Added {playerName} (Team {team}). Total players: {gameManager.GetTeamPlayers().Count}");
        }
        else
        {
            Debug.LogError("SimpleGameManager not found! Please run 'Simple Football/Setup Testing Scene' first.");
        }
    }
    
    [MenuItem("Simple Football/Show Controls")]
    static void ShowControls()
    {
        string controls = @"
🎮 SIMPLE FOOTBALL CONTROLS 🎮

🔧 MOVEMENT:
- W/A/S/D: Move player (keyboard)
- Left Stick: Move player (PS5 controller)
- Left Shift: Run (keyboard)
- Square Button: Run (PS5 controller)

🔄 TEAM CONTROLS:
- Tab: Switch between players (keyboard)
- Triangle Button: Switch between players (PS5 controller)
- Only one player is controllable at a time

🏈 TESTING:
1. Press Play in Unity
2. You'll see TWO blue players - one with a red pulsing circle
3. Use WASD or left stick to move the active player
4. Hold Shift or Square to run faster
5. Press Tab or Triangle to switch between players
6. Camera follows the active player

📝 SETUP STEPS:
1. Run 'Simple Football/Setup Testing Scene'
2. Press Play to test (2 players created automatically)
3. Use 'Add Another Player' to add more players

🛠️ TROUBLESHOOTING:
If you don't see 2 players or the red indicator:
1. Try 'Simple Football/Test Setup (Manual)' instead
2. Check the Console for debug messages
3. Make sure you have a Main Camera in the scene

✅ CONTROLLER SUPPORT:
- PS5 controller support through Unity's standard input
- Left stick for movement, Square for running, Triangle for switching
- Works automatically alongside keyboard controls
- Lower camera angle for better gameplay view

🔴 VISUAL INDICATOR:
- Red pulsing circle shows which player you're controlling
- Ready for team colors (red/blue) in multiplayer

This version works without the Input System package!
        ";
        
        Debug.Log(controls);
    }
    
    [MenuItem("Simple Football/Clean Scene")]
    static void CleanScene()
    {
        if (EditorUtility.DisplayDialog("Clean Scene", 
            "This will remove all simple football objects. Are you sure?", 
            "Yes", "Cancel"))
        {
            // Find and destroy all simple football objects
            GameObject[] gameObjects = FindObjectsOfType<GameObject>();
            
            foreach (GameObject go in gameObjects)
            {
                if (go.GetComponent<SimplePlayerController>() != null ||
                    go.GetComponent<SimpleGameManager>() != null ||
                    go.GetComponent<SimpleCameraController>() != null ||
                    go.GetComponent<SimpleSceneSetup>() != null ||
                    go.name.Contains("Player") ||
                    go.name.Contains("Ground") ||
                    go.name.Contains("Simple"))
                {
                    DestroyImmediate(go);
                }
            }
            
            Debug.Log("Scene cleaned!");
        }
    }
}
#endif 