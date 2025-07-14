using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float followSpeed = 5f;
    public float rotationSpeed = 3f;
    
    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0, 6, -12);
    public float height = 6f;
    public float distance = 12f;
    
    [Header("Look Settings")]
    public bool lookAtTarget = true;
    public Vector3 lookOffset = Vector3.zero;
    
    [Header("Smoothing")]
    public float positionSmoothTime = 0.3f;
    public float rotationSmoothTime = 0.3f;
    
    // Private variables for smooth movement
    private Vector3 velocity;
    private Vector3 rotationVelocity;
    private Vector3 currentRotation;
    private Transform lastTarget;
    private float debugTimer = 0f;
    
    void Start()
    {
        // If no target assigned, try to find the player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Initialize rotation
        currentRotation = transform.eulerAngles;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        FollowTarget();
    }
    
    void FollowTarget()
    {
        // Calculate desired position relative to target
        Vector3 desiredPosition = target.position + offset;
        
        // Debug only when target changes or every 2 seconds
        if (lastTarget != target || Time.time - debugTimer > 2f)
        {
            Debug.Log($"Camera FollowTarget - Target: {target.name}, Target pos: {target.position}, Desired camera pos: {desiredPosition}, Current camera pos: {transform.position}");
            lastTarget = target;
            debugTimer = Time.time;
        }
        
        // Smooth position movement
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);
        transform.position = smoothedPosition;
        
        // Handle rotation if looking at target
        if (lookAtTarget)
        {
            // Calculate direction to target
            Vector3 directionToTarget = (target.position + lookOffset - transform.position).normalized;
            
            // Create rotation that looks at target
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            // Smooth rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    // Public method to switch camera target (for team switching)
    public void SetTarget(Transform newTarget)
    {
        Debug.Log($"Camera SetTarget called. Old target: {(target != null ? target.name : "null")}, New target: {(newTarget != null ? newTarget.name : "null")}");
        target = newTarget;
        Debug.Log($"Camera target set to: {(target != null ? target.name : "null")}");
    }
    
    // Set camera to follow a specific player by ID (for multiplayer/team switching)
    public void FollowPlayer(int playerID)
    {
        SimplePlayerController[] players = FindObjectsOfType<SimplePlayerController>();
        foreach (SimplePlayerController player in players)
        {
            if (player.playerID == playerID)
            {
                SetTarget(player.transform);
                break;
            }
        }
    }
    
    // Set camera to follow the local player (useful for multiplayer)
    public void FollowLocalPlayer()
    {
        SimplePlayerController[] players = FindObjectsOfType<SimplePlayerController>();
        foreach (SimplePlayerController player in players)
        {
            if (player.isLocalPlayer)
            {
                SetTarget(player.transform);
                break;
            }
        }
    }
    
    // Snap camera to target instantly (for testing)
    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 snapPosition = target.position + offset;
            transform.position = snapPosition;
            
            if (lookAtTarget)
            {
                Vector3 directionToTarget = (target.position + lookOffset - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(directionToTarget);
            }
            
            Debug.Log($"Camera snapped to {target.name} at position {snapPosition}");
        }
    }
    
    // Adjust camera settings (useful for different game situations)
    public void SetCameraMode(Vector3 newOffset, float newFollowSpeed = -1)
    {
        offset = newOffset;
        if (newFollowSpeed > 0)
        {
            followSpeed = newFollowSpeed;
        }
    }
} 