using UnityEngine;

public class FootballField : MonoBehaviour
{
    [Header("Field Dimensions")]
    public float fieldLength = 120f; // Total length including endzones
    public float fieldWidth = 53.33f; // Standard football field width
    public float endzoneLength = 10f;
    
    [Header("Materials")]
    public Material fieldMaterial;
    public Material endzoneMaterial;
    public Material linesMaterial;
    
    [Header("Generation Settings")]
    public bool generateOnStart = true;
    public bool createColliders = true;
    
    // References to generated objects
    private GameObject fieldPlane;
    private GameObject[] endzones;
    private GameObject[] fieldLines;
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateField();
        }
    }
    
    public void GenerateField()
    {
        // Clear existing field if regenerating
        ClearField();
        
        // Create main field plane
        CreateFieldPlane();
        
        // Create endzones
        CreateEndzones();
        
        // Create field lines (basic yard lines)
        CreateFieldLines();
        
        // Set up field boundaries
        if (createColliders)
        {
            CreateFieldBoundaries();
        }
    }
    
    void CreateFieldPlane()
    {
        fieldPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        fieldPlane.name = "Football Field";
        fieldPlane.transform.parent = transform;
        
        // Scale the plane to match field dimensions
        // Unity plane is 10x10 units by default
        float scaleX = fieldWidth / 10f;
        float scaleZ = (fieldLength - 2 * endzoneLength) / 10f;
        fieldPlane.transform.localScale = new Vector3(scaleX, 1, scaleZ);
        
        // Apply material
        if (fieldMaterial != null)
        {
            fieldPlane.GetComponent<Renderer>().material = fieldMaterial;
        }
        else
        {
            // Create default green material
            Material defaultMaterial = new Material(Shader.Find("Standard"));
            defaultMaterial.color = new Color(0.2f, 0.6f, 0.2f); // Green color
            fieldPlane.GetComponent<Renderer>().material = defaultMaterial;
        }
        
        // Position the field
        fieldPlane.transform.position = transform.position;
    }
    
    void CreateEndzones()
    {
        endzones = new GameObject[2];
        
        // Create two endzones
        for (int i = 0; i < 2; i++)
        {
            GameObject endzone = GameObject.CreatePrimitive(PrimitiveType.Plane);
            endzone.name = $"Endzone {i + 1}";
            endzone.transform.parent = transform;
            
            // Scale the endzone
            float scaleX = fieldWidth / 10f;
            float scaleZ = endzoneLength / 10f;
            endzone.transform.localScale = new Vector3(scaleX, 1, scaleZ);
            
            // Position the endzone
            float zPosition = (i == 0) ? -(fieldLength - endzoneLength) / 2f : (fieldLength - endzoneLength) / 2f;
            endzone.transform.position = transform.position + new Vector3(0, 0, zPosition);
            
            // Apply material
            if (endzoneMaterial != null)
            {
                endzone.GetComponent<Renderer>().material = endzoneMaterial;
            }
            else
            {
                // Create default darker green material
                Material defaultMaterial = new Material(Shader.Find("Standard"));
                defaultMaterial.color = new Color(0.15f, 0.5f, 0.15f); // Darker green
                endzone.GetComponent<Renderer>().material = defaultMaterial;
            }
            
            endzones[i] = endzone;
        }
    }
    
    void CreateFieldLines()
    {
        // Create basic yard lines every 10 yards
        int numberOfLines = 10; // 10-yard increments
        fieldLines = new GameObject[numberOfLines + 1];
        
        for (int i = 0; i <= numberOfLines; i++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = $"Yard Line {i * 10}";
            line.transform.parent = transform;
            
            // Scale the line
            line.transform.localScale = new Vector3(fieldWidth, 0.1f, 0.5f);
            
            // Position the line
            float fieldPlayLength = fieldLength - 2 * endzoneLength;
            float zPosition = -fieldPlayLength / 2f + (i * fieldPlayLength / numberOfLines);
            line.transform.position = transform.position + new Vector3(0, 0.05f, zPosition);
            
            // Apply material
            if (linesMaterial != null)
            {
                line.GetComponent<Renderer>().material = linesMaterial;
            }
            else
            {
                // Create default white material
                Material defaultMaterial = new Material(Shader.Find("Standard"));
                defaultMaterial.color = Color.white;
                line.GetComponent<Renderer>().material = defaultMaterial;
            }
            
            fieldLines[i] = line;
        }
    }
    
    void CreateFieldBoundaries()
    {
        // Create invisible colliders around the field to keep players in bounds
        GameObject boundaries = new GameObject("Field Boundaries");
        boundaries.transform.parent = transform;
        
        // Create four walls around the field
        float wallHeight = 2f;
        float wallThickness = 1f;
        
        // Front and back walls
        for (int i = 0; i < 2; i++)
        {
            GameObject wall = new GameObject($"Boundary Wall {i + 1}");
            wall.transform.parent = boundaries.transform;
            
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = new Vector3(fieldWidth + wallThickness, wallHeight, wallThickness);
            
            float zPosition = (i == 0) ? -fieldLength / 2f - wallThickness / 2f : fieldLength / 2f + wallThickness / 2f;
            wall.transform.position = transform.position + new Vector3(0, wallHeight / 2f, zPosition);
        }
        
        // Left and right walls
        for (int i = 0; i < 2; i++)
        {
            GameObject wall = new GameObject($"Boundary Wall {i + 3}");
            wall.transform.parent = boundaries.transform;
            
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = new Vector3(wallThickness, wallHeight, fieldLength + wallThickness);
            
            float xPosition = (i == 0) ? -fieldWidth / 2f - wallThickness / 2f : fieldWidth / 2f + wallThickness / 2f;
            wall.transform.position = transform.position + new Vector3(xPosition, wallHeight / 2f, 0);
        }
    }
    
    void ClearField()
    {
        // Clean up existing field objects
        if (fieldPlane != null)
        {
            DestroyImmediate(fieldPlane);
        }
        
        if (endzones != null)
        {
            foreach (GameObject endzone in endzones)
            {
                if (endzone != null)
                {
                    DestroyImmediate(endzone);
                }
            }
        }
        
        if (fieldLines != null)
        {
            foreach (GameObject line in fieldLines)
            {
                if (line != null)
                {
                    DestroyImmediate(line);
                }
            }
        }
        
        // Clear boundary objects
        Transform boundaries = transform.Find("Field Boundaries");
        if (boundaries != null)
        {
            DestroyImmediate(boundaries.gameObject);
        }
    }
    
    // Public method to get field bounds (useful for AI and game logic)
    public Bounds GetFieldBounds()
    {
        return new Bounds(transform.position, new Vector3(fieldWidth, 0, fieldLength));
    }
} 