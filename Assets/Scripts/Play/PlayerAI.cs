using UnityEngine;
using System.Collections;

public class PlayerAI : MonoBehaviour
{
    [Header("AI Settings")]
    public bool isAIControlled = false;
    public float arrivalDistance = 0.5f;
    public float rotationSpeed = 10f;
    public bool allowUserOverride = true;
    
    [Header("Current Route")]
    public PlayerRoute currentRoute;
    public int currentWaypointIndex = 0;
    public bool isRunningRoute = false;
    
    private SimplePlayerController playerController;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 routeStartPosition;
    private float routeSpeed;
    
    void Awake()
    {
        playerController = GetComponent<SimplePlayerController>();
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        if (isAIControlled && isRunningRoute)
        {
            // Check for user input override (only for the currently controlled player)
            if (allowUserOverride && playerController != null && playerController.isLocalPlayer)
            {
                if (CheckForUserInput())
                {
                    Debug.Log($"User input detected for {gameObject.name}, canceling AI route");
                    StopRoute();
                    return;
                }
            }
            
            FollowRoute();
        }
    }
    
    bool CheckForUserInput()
    {
        // Check for any movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // If there's significant input, user wants to override AI
        bool hasInput = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        
        if (hasInput)
        {
            Debug.Log($"User input detected for {gameObject.name}: H={horizontal:F2}, V={vertical:F2}");
        }
        
        return hasInput;
    }
    
    public void StartRoute(PlayerRoute route)
    {
        if (route == null || route.waypoints == null || route.waypoints.Length == 0)
        {
            Debug.LogWarning($"Invalid route for {gameObject?.name ?? "Unknown"}");
            return;
        }
        
        if (transform == null)
        {
            Debug.LogError("PlayerAI transform is null, cannot start route");
            return;
        }
        
        currentRoute = route;
        currentWaypointIndex = 0;
        isRunningRoute = true;
        isAIControlled = true;
        routeSpeed = route.speed;
        
        // Store starting position for waypoint calculations
        routeStartPosition = transform.position;
        
        // Set first target (waypoints are relative to starting position)
        targetPosition = routeStartPosition + route.waypoints[0];
        
        // Disable player control temporarily
        if (playerController != null)
        {
            playerController.SetAIControlled(true);
        }
        
        Debug.Log($"{gameObject.name} starting AI route from {routeStartPosition} to {targetPosition} (waypoint: {route.waypoints[0]})");
    }
    
    public void StopRoute()
    {
        isRunningRoute = false;
        isAIControlled = false;
        
        // Re-enable player control
        if (playerController != null)
        {
            playerController.SetAIControlled(false);
            Debug.Log($"{gameObject.name} AI route stopped - player control restored");
        }
        
        Debug.Log($"{gameObject.name} stopped AI route");
    }
    
    void FollowRoute()
    {
        if (currentRoute == null || currentRoute.waypoints == null)
            return;
        
        // Check if we've reached the current waypoint
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (distanceToTarget < arrivalDistance)
        {
            // Move to next waypoint
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= currentRoute.waypoints.Length)
            {
                // Route completed
                CompleteRoute();
                return;
            }
            
            // Set new target (waypoints are relative to starting position)
            targetPosition = routeStartPosition + currentRoute.waypoints[currentWaypointIndex];
            Debug.Log($"{gameObject.name} moving to waypoint {currentWaypointIndex}: {targetPosition} (offset: {currentRoute.waypoints[currentWaypointIndex]})");
        }
        
        // Move towards target
        MoveTowardsTarget();
    }
    
    void MoveTowardsTarget()
    {
        // Calculate direction to target
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep on ground
        
        // Move using rigidbody
        Vector3 movement = direction * routeSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
        
        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void CompleteRoute()
    {
        isRunningRoute = false;
        Debug.Log($"{gameObject.name} completed AI route");
        
        // Don't automatically stop AI control - let user input override handle it
        // Players will keep their final position until user takes control
        Debug.Log($"{gameObject.name} route completed but staying under AI control until user input");
    }
    
    public bool IsRunningRoute()
    {
        return isRunningRoute;
    }
    
    public float GetRouteProgress()
    {
        if (currentRoute == null || currentRoute.waypoints == null)
            return 0f;
        
        return (float)currentWaypointIndex / currentRoute.waypoints.Length;
    }
    
    public Vector3 GetCurrentTarget()
    {
        return targetPosition;
    }
    
    public Vector3 GetCurrentVelocity()
    {
        if (!isRunningRoute || currentRoute == null)
            return Vector3.zero;
        
        // Calculate current movement direction
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep on ground
        
        // Return velocity based on route speed
        return direction * routeSpeed;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (currentRoute != null && currentRoute.waypoints != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 startPos = isRunningRoute ? routeStartPosition : transform.position;
            Vector3 lastPos = startPos;
            
            for (int i = 0; i < currentRoute.waypoints.Length; i++)
            {
                Vector3 waypointPos = startPos + currentRoute.waypoints[i];
                Gizmos.DrawWireSphere(waypointPos, 0.3f);
                
                if (i == 0)
                {
                    Gizmos.DrawLine(startPos, waypointPos);
                }
                else
                {
                    Gizmos.DrawLine(lastPos, waypointPos);
                }
                
                lastPos = waypointPos;
            }
            
            // Highlight current target
            if (isRunningRoute)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(targetPosition, 0.5f);
                
                // Draw line from current position to target
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetPosition);
            }
        }
    }
} 