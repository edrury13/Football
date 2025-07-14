using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    public float circleRadius = 2f; // Increased from 1.5f to make it more visible
    public float circleHeight = 0.02f; // Increased from 0.01f to make it more visible
    public Color activeColor = Color.red;
    public Color inactiveColor = Color.clear;
    
    [Header("Animation")]
    public bool animateIndicator = true;
    public float pulseSpeed = 3f; // Increased from 2f to make it more noticeable
    public float minAlpha = 0.5f; // Increased from 0.3f to make it more visible
    public float maxAlpha = 1f;
    
    private GameObject indicatorObject;
    private Renderer indicatorRenderer;
    private Material indicatorMaterial;
    private bool isActive = false;
    
    // Public getter for isActive
    public bool IsActive => isActive;
    
    void Awake()
    {
        CreateIndicator();
    }
    
    void Update()
    {
        if (isActive && animateIndicator)
        {
            AnimateIndicator();
        }
    }
    
    void CreateIndicator()
    {
        Debug.Log($"Creating indicator for {gameObject.name}");
        
        // Create a flat cylinder as the indicator
        indicatorObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicatorObject.name = "Player Indicator";
        indicatorObject.transform.parent = transform;
        
        // Position and scale the indicator
        indicatorObject.transform.localPosition = new Vector3(0, 0.01f, 0);
        indicatorObject.transform.localScale = new Vector3(circleRadius, circleHeight, circleRadius);
        
        // Remove the collider (we don't want it to interfere with physics)
        Collider indicatorCollider = indicatorObject.GetComponent<Collider>();
        if (indicatorCollider != null)
        {
            DestroyImmediate(indicatorCollider);
        }
        
        // Create and configure the material (simplified)
        indicatorRenderer = indicatorObject.GetComponent<Renderer>();
        indicatorMaterial = new Material(Shader.Find("Standard"));
        indicatorMaterial.color = activeColor; // Start with active color for testing
        
        // Make it emissive so it's more visible
        indicatorMaterial.EnableKeyword("_EMISSION");
        indicatorMaterial.SetColor("_EmissionColor", activeColor * 0.5f);
        
        indicatorRenderer.material = indicatorMaterial;
        
        Debug.Log($"Indicator created for {gameObject.name}");
        
        // Start inactive
        SetActive(false);
    }
    
    void AnimateIndicator()
    {
        if (indicatorMaterial != null)
        {
            // Pulsing alpha animation
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color color = activeColor;
            color.a = alpha;
            indicatorMaterial.color = color;
        }
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        
        Debug.Log($"Setting indicator {active} for {gameObject.name} at position {transform.position}");
        
        // Ensure indicator is created before using it
        if (indicatorObject == null)
        {
            CreateIndicator();
        }
        
        if (indicatorMaterial != null)
        {
            if (active)
            {
                indicatorMaterial.color = activeColor;
                indicatorMaterial.SetColor("_EmissionColor", activeColor * 0.5f);
                Debug.Log($"Indicator activated for {gameObject.name} - color set to {activeColor}");
            }
            else
            {
                indicatorMaterial.color = inactiveColor;
                indicatorMaterial.SetColor("_EmissionColor", Color.black);
                Debug.Log($"Indicator deactivated for {gameObject.name}");
            }
        }
        else
        {
            Debug.LogError($"Indicator material is null for {gameObject.name}!");
        }
        
        // Also control the visibility of the indicator object itself
        if (indicatorObject != null)
        {
            indicatorObject.SetActive(active);
            Debug.Log($"Indicator object active state set to {active} for {gameObject.name}");
        }
        else
        {
            Debug.LogError($"Indicator object is null for {gameObject.name}!");
        }
    }
    
    public void SetColor(Color color)
    {
        activeColor = color;
        if (isActive && indicatorMaterial != null)
        {
            indicatorMaterial.color = activeColor;
        }
    }
    
    public void SetRadius(float radius)
    {
        circleRadius = radius;
        if (indicatorObject != null)
        {
            indicatorObject.transform.localScale = new Vector3(circleRadius, circleHeight, circleRadius);
        }
    }
} 