using UnityEngine;

public class FootballBall : MonoBehaviour
{
    [Header("Ball Settings")]
    public float throwForce = 15f;
    public float catchRadius = 1f;
    public float snapSpeed = 8f;
    
    [Header("Visual Settings")]
    public Material ballMaterial;
    
    public SimplePlayerController currentCarrier { get; private set; }
    public bool isSnapped { get; private set; } = false;
    public bool isInAir { get; private set; } = false;
    
    private Rigidbody ballRigidbody;
    private bool isBeingCarried = false;
    private Vector3 carryOffset = new Vector3(0, 1.5f, 0.5f);
    
    // Events
    public System.Action<SimplePlayerController> OnBallCarrierChanged;
    public System.Action OnBallSnapped;
    public System.Action OnBallCaught;
    
    void Awake()
    {
        Debug.Log("FootballBall Awake called");
        
        ballRigidbody = GetComponent<Rigidbody>();
        if (ballRigidbody == null)
        {
            ballRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure ball physics
        ballRigidbody.mass = 0.4f;
        ballRigidbody.drag = 0.3f;
        ballRigidbody.angularDrag = 0.2f;
        ballRigidbody.useGravity = true;
        
        // Set up ball visual
        SetupBallVisual();
        
        Debug.Log($"FootballBall setup complete at position {transform.position}");
    }
    
    void Start()
    {
        // Find the center player and give them the ball initially
        SimplePlayerController[] players = FindObjectsOfType<SimplePlayerController>();
        foreach (var player in players)
        {
            if (player.position == PlayerPosition.C) // Center
            {
                GiveBallToPlayer(player);
                Debug.Log($"Ball given to center player at position: {player.transform.position}");
                break;
            }
        }
        
        // If no center found, just place the ball at the line of scrimmage
        if (currentCarrier == null)
        {
            transform.position = new Vector3(0, 1f, 0);
            Debug.Log("No center found, ball placed at line of scrimmage");
        }
    }
    
    void Update()
    {
        if (isBeingCarried && currentCarrier != null)
        {
            // Follow the carrier
            transform.position = currentCarrier.transform.position + carryOffset;
            transform.rotation = currentCarrier.transform.rotation;
        }
        else if (isInAir)
        {
            // Ball is in flight, check for catches continuously
            CheckForCatch();
        }
        else if (!isBeingCarried && ballRigidbody.velocity.magnitude < 0.1f)
        {
            // Ball has stopped, check for nearby players to catch it
            CheckForCatch();
        }
    }
    
    void SetupBallVisual()
    {
        // Create ball visual directly on this GameObject
        if (GetComponent<MeshRenderer>() == null)
        {
            // Add mesh components
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            // Create a sphere mesh and scale it to football shape
            meshFilter.mesh = CreateEllipsoidMesh();
            
            // Apply material
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.6f, 0.3f, 0.1f); // Brown color
            meshRenderer.material = material;
            
            // Add collider
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.3f;
            collider.isTrigger = true;
            
            Debug.Log("Football visual created successfully");
        }
    }
    
    Mesh CreateEllipsoidMesh()
    {
        // Create a basic sphere mesh and scale it
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh sphereMesh = tempSphere.GetComponent<MeshFilter>().mesh;
        
        // Create a copy of the mesh
        Mesh footballMesh = new Mesh();
        footballMesh.vertices = sphereMesh.vertices;
        footballMesh.triangles = sphereMesh.triangles;
        footballMesh.normals = sphereMesh.normals;
        footballMesh.uv = sphereMesh.uv;
        
        // Scale vertices to create football shape
        Vector3[] vertices = footballMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x * 0.3f, vertices[i].y * 0.5f, vertices[i].z * 0.3f);
        }
        footballMesh.vertices = vertices;
        footballMesh.RecalculateNormals();
        
        // Clean up temp object
        DestroyImmediate(tempSphere);
        
        return footballMesh;
    }
    
    public void SnapBall()
    {
        if (isSnapped) return;
        
        isSnapped = true;
        isInAir = true;
        isBeingCarried = false;
        
        // Find QB and snap to them
        SimplePlayerController[] players = FindObjectsOfType<SimplePlayerController>();
        SimplePlayerController qb = null;
        
        foreach (var player in players)
        {
            if (player.position == PlayerPosition.QB)
            {
                qb = player;
                break;
            }
        }
        
        if (qb != null)
        {
            // Animate ball to QB
            StartCoroutine(SnapToPlayer(qb));
        }
        
        OnBallSnapped?.Invoke();
        Debug.Log("Ball snapped!");
    }
    
    System.Collections.IEnumerator SnapToPlayer(SimplePlayerController targetPlayer)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetPlayer.transform.position + carryOffset;
        float snapTime = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < snapTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / snapTime;
            
            // Smooth curve for the snap
            float smoothT = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            
            yield return null;
        }
        
        // Ball caught by QB
        GiveBallToPlayer(targetPlayer);
        isInAir = false;
    }
    
    public void GiveBallToPlayer(SimplePlayerController player)
    {
        if (currentCarrier == player) return;
        
        bool wasBallInAir = isInAir; // Track if this was a catch
        
        currentCarrier = player;
        isBeingCarried = true;
        isInAir = false;
        
        // Stop ball physics
        ballRigidbody.isKinematic = true;
        ballRigidbody.velocity = Vector3.zero;
        
        // Position the ball with the player
        transform.position = player.transform.position + carryOffset;
        
        // If this was a catch (ball was in air), switch camera and control to receiver
        if (wasBallInAir)
        {
            // Switch camera to receiver
            SimpleCameraController cameraController = FindObjectOfType<SimpleCameraController>();
            if (cameraController != null)
            {
                cameraController.SetTarget(player.transform);
                Debug.Log($"Camera switched to receiver: {player.playerName}");
            }
            
            // Switch player control to receiver
            SimpleGameManager gameManager = SimpleGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.SwitchToPlayer(player);
                Debug.Log($"Player control switched to receiver: {player.playerName}");
            }
        }
        
        OnBallCarrierChanged?.Invoke(player);
        Debug.Log($"Ball given to {player.playerName} at position {transform.position}");
    }
    
    public void ThrowBall(Vector3 direction, float force)
    {
        if (!isBeingCarried) return;
        
        currentCarrier = null;
        isBeingCarried = false;
        isInAir = true;
        
        // Enable physics
        ballRigidbody.isKinematic = false;
        ballRigidbody.velocity = direction * force;
        
        Debug.Log("Ball thrown!");
    }
    
    public void PassBall(Vector3 inputDirection)
    {
        if (!isBeingCarried || currentCarrier == null) return;
        
        // Find the best target based on input direction
        SimplePlayerController targetPlayer = FindBestPassTarget(inputDirection);
        
        if (targetPlayer != null)
        {
            // Calculate throw direction with leading
            Vector3 throwDirection = CalculateLeadingThrow(targetPlayer);
            float throwForce = CalculateThrowForce(targetPlayer);
            
            Debug.Log($"Passing ball to {targetPlayer.playerName} with direction {throwDirection}");
            
            // Store reference to old carrier
            SimplePlayerController oldCarrier = currentCarrier;
            
            // Throw the ball
            currentCarrier = null;
            isBeingCarried = false;
            isInAir = true;
            
            // Enable physics
            ballRigidbody.isKinematic = false;
            ballRigidbody.velocity = throwDirection * throwForce;
            
            // Make camera follow the ball during flight
            SimpleCameraController cameraController = FindObjectOfType<SimpleCameraController>();
            if (cameraController != null)
            {
                cameraController.SetTarget(transform);
                Debug.Log("Camera now following ball during flight");
            }
            
            Debug.Log($"Ball passed to {targetPlayer.playerName}!");
        }
        else
        {
            Debug.Log("No valid pass target found!");
        }
    }
    
    SimplePlayerController FindBestPassTarget(Vector3 inputDirection)
    {
        if (inputDirection.magnitude < 0.1f) return null; // No input
        
        SimplePlayerController[] players = FindObjectsOfType<SimplePlayerController>();
        SimplePlayerController bestTarget = null;
        float bestScore = float.MinValue;
        
        foreach (var player in players)
        {
            // Skip QB and non-receiver positions
            if (player.position == PlayerPosition.QB || 
                player.position == PlayerPosition.C ||
                player.position == PlayerPosition.LG ||
                player.position == PlayerPosition.RG ||
                player.position == PlayerPosition.LT ||
                player.position == PlayerPosition.RT)
            {
                continue;
            }
            
            Vector3 playerDirection = (player.transform.position - currentCarrier.transform.position).normalized;
            
            // Calculate alignment with input direction
            float dotProduct = Vector3.Dot(inputDirection.normalized, playerDirection);
            
            // Consider distance (closer is better for short passes)
            float distance = Vector3.Distance(currentCarrier.transform.position, player.transform.position);
            float distanceScore = 1f / (1f + distance * 0.1f);
            
            // Combined score
            float score = dotProduct * 0.7f + distanceScore * 0.3f;
            
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = player;
            }
        }
        
        return bestTarget;
    }
    
    Vector3 CalculateLeadingThrow(SimplePlayerController target)
    {
        // Get target's current velocity from PlayerAI if available
        PlayerAI targetAI = target.GetComponent<PlayerAI>();
        Vector3 targetVelocity = Vector3.zero;
        
        if (targetAI != null && targetAI.enabled)
        {
            // If AI is controlling the player, use their current direction and speed
            targetVelocity = targetAI.GetCurrentVelocity();
        }
        else
        {
            // Fallback: estimate velocity from movement
            targetVelocity = target.GetComponent<Rigidbody>()?.velocity ?? Vector3.zero;
        }
        
        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.transform.position;
        
        // Calculate interception point
        float ballSpeed = throwForce;
        float distance = Vector3.Distance(currentPos, targetPos);
        float timeToTarget = distance / ballSpeed;
        
        // Lead the target
        Vector3 leadPosition = targetPos + (targetVelocity * timeToTarget);
        
        // Calculate throw direction with arc
        Vector3 throwDirection = (leadPosition - currentPos).normalized;
        
        // Add upward arc for more realistic passing
        float arcHeight = Mathf.Clamp(distance * 0.1f, 2f, 8f);
        throwDirection.y += arcHeight / distance;
        throwDirection = throwDirection.normalized;
        
        return throwDirection;
    }
    
    float CalculateThrowForce(SimplePlayerController target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        
        // Adjust force based on distance - stronger throws for longer distances
        float baseForce = throwForce;
        float distanceMultiplier = Mathf.Clamp(distance / 15f, 0.8f, 2.5f);
        
        // Account for gravity - need more force for longer throws
        float gravityCompensation = 1f + (distance * 0.05f);
        
        return baseForce * distanceMultiplier * gravityCompensation;
    }
    
    void CheckForCatch()
    {
        SimplePlayerController[] players = FindObjectsOfType<SimplePlayerController>();
        SimplePlayerController closestPlayer = null;
        float closestDistance = float.MaxValue;
        
        foreach (var player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < catchRadius && distance < closestDistance)
            {
                closestPlayer = player;
                closestDistance = distance;
            }
        }
        
        if (closestPlayer != null)
        {
            GiveBallToPlayer(closestPlayer);
            OnBallCaught?.Invoke();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isBeingCarried) return;
        
        SimplePlayerController player = other.GetComponent<SimplePlayerController>();
        if (player != null)
        {
            GiveBallToPlayer(player);
        }
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (isInAir)
        {
            // Draw ball trajectory
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, catchRadius);
            
            // Draw velocity vector
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, ballRigidbody.velocity.normalized * 3f);
        }
        else if (isBeingCarried)
        {
            // Draw catch radius around carried ball
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, catchRadius);
        }
    }
} 