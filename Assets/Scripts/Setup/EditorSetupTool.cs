#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class EditorSetupTool : MonoBehaviour
{
    [MenuItem("Football Game/Setup Scene")]
    static void SetupFootballScene()
    {
        // Create setup object
        GameObject setupObject = new GameObject("Scene Setup");
        SceneSetupGuide setupGuide = setupObject.AddComponent<SceneSetupGuide>();
        
        // Run the setup
        setupGuide.SetupScene();
        
        // Select the setup object in the hierarchy
        Selection.activeGameObject = setupObject;
        
        Debug.Log("Football scene setup initiated! Check the console for instructions.");
    }
    
    [MenuItem("Football Game/Add Additional Player")]
    static void AddAdditionalPlayer()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            Vector3 spawnPosition = Vector3.zero;
            if (gameManager.GetField() != null)
            {
                spawnPosition = gameManager.GetField().transform.position + new Vector3(5, 1, 0);
            }
            
            gameManager.AddPlayer(spawnPosition, $"Player {gameManager.GetTeamPlayers().Count + 1}");
            Debug.Log($"Added additional player. Total players: {gameManager.GetTeamPlayers().Count}");
        }
        else
        {
            Debug.LogError("GameManager not found! Please run 'Football Game/Setup Scene' first.");
        }
    }
    
    [MenuItem("Football Game/Show Controls")]
    static void ShowControls()
    {
        string controls = @"
🎮 FOOTBALL GAME CONTROLS 🎮

🔧 MOVEMENT:
- W/A/S/D or Left Stick: Move player
- Left Shift or Right Trigger: Run (hold)

🔄 TEAM CONTROLS:
- Tab or Y Button (Xbox): Switch between players
- Only one player can be controlled at a time

🏈 GAME SETUP:
- Use 'Football Game/Setup Scene' to create the field and initial player
- Use 'Football Game/Add Additional Player' to add more team members
- Camera automatically follows the active player

📝 NEXT STEPS:
1. Test basic movement with WASD
2. Add more players and test team switching with Tab
3. Implement ball mechanics and passing
4. Add multiplayer support with Mirror Networking
        ";
        
        Debug.Log(controls);
    }
    
    [MenuItem("Football Game/Reset Scene")]
    static void ResetScene()
    {
        if (EditorUtility.DisplayDialog("Reset Scene", 
            "This will remove all football game objects from the scene. Are you sure?", 
            "Yes", "Cancel"))
        {
            // Find and destroy all football-related objects
            GameObject[] gameObjects = FindObjectsOfType<GameObject>();
            
            foreach (GameObject go in gameObjects)
            {
                if (go.GetComponent<FootballField>() != null ||
                    go.GetComponent<GameManager>() != null ||
                    go.GetComponent<PlayerController>() != null ||
                    go.GetComponent<SceneSetupGuide>() != null ||
                    go.name.Contains("Football") ||
                    go.name.Contains("Player"))
                {
                    DestroyImmediate(go);
                }
            }
            
            Debug.Log("Scene reset complete!");
        }
    }
}
#endif 