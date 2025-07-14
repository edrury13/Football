using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayManager : MonoBehaviour
{
    [Header("Play Settings")]
    public PlayData currentPlay;
    public bool isPlayActive = false;
    public float playStartTime;
    
    [Header("References")]
    public SimpleGameManager gameManager;
    public FootballBall football;
    public SimpleTestSetup testSetup;
    
    // Player AI tracking
    private Dictionary<PlayerPosition, PlayerAI> playerAIs = new Dictionary<PlayerPosition, PlayerAI>();
    private Dictionary<PlayerPosition, SimplePlayerController> playerControllers = new Dictionary<PlayerPosition, SimplePlayerController>();
    
    // Handoff system
    private PlayerRoute handoffGiver;
    private PlayerRoute handoffTarget;
    private bool handoffCompleted = false;
    
    // Events
    public System.Action<PlayData> OnPlayStarted;
    public System.Action<PlayData> OnPlayCompleted;
    public System.Action OnHandoffCompleted;
    
    void Awake()
    {
        // Find references
        if (gameManager == null)
            gameManager = FindObjectOfType<SimpleGameManager>();
        if (football == null)
            football = FindObjectOfType<FootballBall>();
        if (testSetup == null)
            testSetup = FindObjectOfType<SimpleTestSetup>();
    }
    
    void OnEnable()
    {
        // Subscribe to handoff completion events
        OnHandoffCompleted += HandleHandoffCompleted;
    }
    
    void OnDisable()
    {
        // Unsubscribe from events
        OnHandoffCompleted -= HandleHandoffCompleted;
    }
    
    void HandleHandoffCompleted()
    {
        Debug.Log("Handoff completed - ensuring ball carrier gets control but keeping AI routes active");
        
        // Make sure the game manager switches control to the ball carrier
        if (gameManager != null && football != null && football.currentCarrier != null)
        {
            // The SimpleGameManager should automatically switch control via OnBallCarrierChanged
            // But let's verify it happened
            SimplePlayerController ballCarrier = football.currentCarrier;
            if (!ballCarrier.isLocalPlayer)
            {
                Debug.LogWarning($"Ball carrier {ballCarrier.playerName} doesn't have local control - forcing switch");
                // Force the switch if it didn't happen automatically
                if (gameManager.teamPlayers.Contains(ballCarrier))
                {
                    gameManager.SwitchToPlayer(ballCarrier);
                }
            }
            
            // Ensure the ball carrier can override AI with input
            if (playerAIs.ContainsKey(ballCarrier.position))
            {
                PlayerAI ballCarrierAI = playerAIs[ballCarrier.position];
                ballCarrierAI.allowUserOverride = true;
                Debug.Log($"Ball carrier {ballCarrier.playerName} can now override AI with input");
            }
        }
    }
    
    void Start()
    {
        // Initialize player tracking
        InitializePlayerTracking();
        
        // Subscribe to play start event
        if (gameManager != null)
        {
            gameManager.OnPlayStarted += OnBallSnapped;
        }
    }
    
    void InitializePlayerTracking()
    {
        // Clear existing dictionaries
        playerControllers.Clear();
        playerAIs.Clear();
        
        SimplePlayerController[] allPlayers = FindObjectsOfType<SimplePlayerController>();
        
        if (allPlayers.Length == 0)
        {
            Debug.LogWarning("No SimplePlayerController components found in scene!");
            return;
        }
        
        foreach (var player in allPlayers)
        {
            if (player == null)
            {
                Debug.LogWarning("Found null player in scene!");
                continue;
            }
            
            playerControllers[player.position] = player;
            
            // Add AI component to each player
            PlayerAI ai = player.GetComponent<PlayerAI>();
            if (ai == null)
            {
                ai = player.gameObject.AddComponent<PlayerAI>();
                Debug.Log($"Added PlayerAI component to {player.playerName}");
            }
            
            if (ai != null)
            {
                playerAIs[player.position] = ai;
                Debug.Log($"Registered AI for {player.position}: {player.playerName}");
            }
            else
            {
                Debug.LogError($"Failed to add PlayerAI component to {player.playerName}");
            }
        }
        
        Debug.Log($"PlayManager initialized with {playerControllers.Count} players and {playerAIs.Count} AI components");
    }
    
    void OnBallSnapped()
    {
        // Reinitialize player tracking to ensure all components are valid
        InitializePlayerTracking();
        
        // Start the selected play when ball is snapped
        PlayData playToRun = null;
        
        // Try to get play from GameManager first
        if (gameManager != null)
        {
            playToRun = gameManager.GetCurrentPlay();
            if (playToRun != null)
            {
                Debug.Log($"Using selected play from GameManager: {playToRun.playName}");
            }
        }
        
        // Fallback to testSetup
        if (playToRun == null && testSetup != null && testSetup.GetSelectedPlay() != null)
        {
            playToRun = testSetup.GetSelectedPlay();
            Debug.Log($"Using selected play from TestSetup: {playToRun.playName}");
        }
        
        // Final fallback to default play
        if (playToRun == null)
        {
            playToRun = PlayBook.GetOutsidePlay();
            Debug.Log("No play selected, using default Outside play");
        }
        
        StartPlay(playToRun);
    }
    
    public void StartPlay(PlayData play)
    {
        if (isPlayActive)
        {
            Debug.LogWarning("Play already active, cannot start new play");
            return;
        }
        
        currentPlay = play;
        isPlayActive = true;
        playStartTime = Time.time;
        handoffCompleted = false;
        
        Debug.Log($"Starting play: {play.playName} (Formation: {play.formation.formationName}, Type: {play.playType})");
        
        // Find handoff participants (only for run plays)
        handoffGiver = null;
        handoffTarget = null;
        
        if (play.IsRunPlay())
        {
            foreach (var route in play.playerRoutes)
            {
                if (route.isHandoffGiver)
                    handoffGiver = route;
                if (route.isHandoffTarget)
                    handoffTarget = route;
            }
        }
        
        // Start AI for all players
        StartCoroutine(ExecutePlay());
        
        OnPlayStarted?.Invoke(play);
    }
    
    IEnumerator ExecutePlay()
    {
        // Start all player routes
        foreach (var route in currentPlay.playerRoutes)
        {
            if (playerAIs.ContainsKey(route.playerPosition))
            {
                StartCoroutine(ExecutePlayerRoute(route));
            }
        }
        
        // Handle handoff timing (only for run plays)
        if (currentPlay.IsRunPlay() && handoffGiver != null && handoffTarget != null)
        {
            StartCoroutine(HandleHandoff());
            Debug.Log($"Starting handoff system for run play: {currentPlay.playName}");
        }
        else if (currentPlay.IsPassPlay())
        {
            Debug.Log($"Pass play detected: {currentPlay.playName} - no handoff system");
        }
        
        // Wait for play to complete
        yield return new WaitForSeconds(currentPlay.playDuration);
        
        // End play
        EndPlay();
    }
    
    IEnumerator ExecutePlayerRoute(PlayerRoute route)
    {
        if (!playerAIs.ContainsKey(route.playerPosition))
        {
            Debug.LogWarning($"No AI found for player position: {route.playerPosition}");
            yield break;
        }
        
        PlayerAI ai = playerAIs[route.playerPosition];
        SimplePlayerController controller = playerControllers[route.playerPosition];
        
        // Check if components are valid
        if (ai == null || controller == null)
        {
            Debug.LogError($"Missing components for {route.playerPosition}: AI={ai != null}, Controller={controller != null}");
            yield break;
        }
        
        // Wait for start delay
        yield return new WaitForSeconds(route.delayBeforeStart);
        
        // Execute the route
        try
        {
            ai.StartRoute(route);
            Debug.Log($"{route.playerPosition} starting route with {route.waypoints.Length} waypoints");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error starting route for {route.playerPosition}: {e.Message}");
        }
    }
    
    IEnumerator HandleHandoff()
    {
        if (handoffGiver == null || handoffTarget == null)
        {
            Debug.LogError("Handoff failed: Missing handoff route data");
            yield break;
        }
        
        if (!playerControllers.ContainsKey(handoffGiver.playerPosition) || 
            !playerControllers.ContainsKey(handoffTarget.playerPosition))
        {
            Debug.LogError("Handoff failed: Player positions not found in controllers dictionary");
            yield break;
        }
        
        SimplePlayerController giverController = playerControllers[handoffGiver.playerPosition];
        SimplePlayerController targetController = playerControllers[handoffTarget.playerPosition];
        
        if (giverController == null || targetController == null)
        {
            Debug.LogError($"Handoff failed: Missing player controllers - Giver: {giverController != null}, Target: {targetController != null}");
            yield break;
        }
        
        Debug.Log($"Handoff system started - waiting for timing and distance");
        Debug.Log($"QB position: {giverController.transform.position}, RB position: {targetController.transform.position}");
        
        // Wait for minimum handoff timing
        yield return new WaitForSeconds(handoffGiver.handoffTiming);
        
        // Keep checking for handoff opportunity (up to 3 seconds)
        float maxWaitTime = 3f;
        float elapsed = 0f;
        
        while (elapsed < maxWaitTime && !handoffCompleted)
        {
            float distance = Vector3.Distance(giverController.transform.position, targetController.transform.position);
            
            Debug.Log($"Handoff check: distance={distance:F2}m, QB has ball={football?.currentCarrier == giverController}");
            
            if (distance < 4f) // Increased handoff range
            {
                // Execute handoff
                if (football != null && football.currentCarrier == giverController)
                {
                    football.GiveBallToPlayer(targetController);
                    handoffCompleted = true;
                    
                    // Don't stop AI routes - let players continue until user input
                    
                    Debug.Log($"Handoff completed: {handoffGiver.playerPosition} to {handoffTarget.playerPosition}");
                    OnHandoffCompleted?.Invoke();
                    break;
                }
                else
                {
                    Debug.LogWarning($"Handoff failed: QB doesn't have ball. Current carrier: {football?.currentCarrier?.playerName ?? "None"}");
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!handoffCompleted)
        {
            Debug.LogWarning($"Handoff timed out after {maxWaitTime}s");
        }
    }
    
    void EndPlay()
    {
        isPlayActive = false;
        
        // Stop all AI with null checks
        foreach (var ai in playerAIs.Values)
        {
            if (ai != null)
            {
                try
                {
                    ai.StopRoute();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error stopping AI route: {e.Message}");
                }
            }
        }
        
        Debug.Log($"Play ended: {currentPlay?.playName ?? "Unknown"}");
        OnPlayCompleted?.Invoke(currentPlay);
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnPlayStarted -= OnBallSnapped;
        }
    }
    
    // Public methods for future extension
    public void SetCustomPlay(PlayData play)
    {
        if (!isPlayActive)
        {
            // Could be used for play selection in the future
            Debug.Log($"Custom play set: {play.playName}");
        }
    }
    
    public bool IsPlayActive()
    {
        return isPlayActive;
    }
    
    public float GetPlayElapsedTime()
    {
        return isPlayActive ? Time.time - playStartTime : 0f;
    }
} 