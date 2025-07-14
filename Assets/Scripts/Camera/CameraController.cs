using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float followSpeed = 5f;
    public float rotationSpeed = 3f;
    
    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0, 8, -10);
    public float height = 8f;
    public float distance = 10f;
    
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
        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Smooth position movement
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);
        transform.position = smoothedPosition;
        
        // Handle rotation if looking at target
        if (lookAtTarget)
        {
            Vector3 lookDirection = (target.position + lookOffset - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            
            // Smooth rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    // Public method to switch camera target (for team switching)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // Set camera to follow a specific player by ID (for multiplayer/team switching)
    public void FollowPlayer(int playerID)
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
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
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            if (player.isLocalPlayer)
            {
                SetTarget(player.transform);
                break;
            }
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