using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlaySelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas playSelectionCanvas;
    public Button playButtonPrefab;
    public Transform playButtonContainer;
    public Text instructionText;
    public Text selectedPlayText;
    
    [Header("Play Selection")]
    public PlayData selectedPlay;
    public bool playSelected = false;
    
    // Events
    public System.Action<PlayData> OnPlaySelected;
    
    // Available plays
    private List<PlayData> availablePlays = new List<PlayData>();
    
    // Navigation
    private int currentSelectionIndex = 0;
    private List<Button> playButtons = new List<Button>();
    private float nextInputTime = 0f;
    private const float inputCooldown = 0.2f;
    
    void Start()
    {
        SetupUI();
        LoadAvailablePlays();
        CreatePlayButtons();
        UpdateSelection();
        
        // Start with UI visible for play selection
        ShowPlaySelection();
    }
    
    void Update()
    {
        // Only handle input if the canvas is active
        if (!playSelectionCanvas.gameObject.activeInHierarchy) return;
        
        // Handle input with cooldown
        if (Time.time < nextInputTime) return;
        
        HandleNavigationInput();
        HandleSelectionInput();
    }
    
    void SetupUI()
    {
        // Create canvas if it doesn't exist
        if (playSelectionCanvas == null)
        {
            GameObject canvasObject = new GameObject("Play Selection Canvas");
            playSelectionCanvas = canvasObject.AddComponent<Canvas>();
            playSelectionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            playSelectionCanvas.sortingOrder = 100;
            
            // Add GraphicRaycaster for button interactions
            canvasObject.AddComponent<GraphicRaycaster>();
            
            // Add CanvasScaler for proper scaling
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        
        // Ensure EventSystem exists for UI interactions
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
            Debug.Log("EventSystem created for UI interactions");
        }
        
        // Create instruction text
        if (instructionText == null)
        {
            GameObject textObject = new GameObject("Instruction Text");
            textObject.transform.SetParent(playSelectionCanvas.transform);
            instructionText = textObject.AddComponent<Text>();
            instructionText.text = "SELECT A PLAY\nUse W/S or Left Stick to navigate\nPress Tab or X to select";
            instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            instructionText.fontSize = 36;
            instructionText.alignment = TextAnchor.MiddleCenter;
            instructionText.color = Color.white;
            
            // Position at top of screen
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.8f);
            textRect.anchorMax = new Vector2(1, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        // Create selected play text
        if (selectedPlayText == null)
        {
            GameObject selectedTextObject = new GameObject("Selected Play Text");
            selectedTextObject.transform.SetParent(playSelectionCanvas.transform);
            selectedPlayText = selectedTextObject.AddComponent<Text>();
            selectedPlayText.text = "";
            selectedPlayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            selectedPlayText.fontSize = 32;
            selectedPlayText.alignment = TextAnchor.MiddleCenter;
            selectedPlayText.color = Color.green;
            
            // Position at bottom of screen
            RectTransform selectedTextRect = selectedTextObject.GetComponent<RectTransform>();
            selectedTextRect.anchorMin = new Vector2(0, 0f);
            selectedTextRect.anchorMax = new Vector2(1, 0.2f);
            selectedTextRect.offsetMin = Vector2.zero;
            selectedTextRect.offsetMax = Vector2.zero;
            
            selectedTextObject.SetActive(false);
        }
        
        // Create button container
        if (playButtonContainer == null)
        {
            GameObject containerObject = new GameObject("Play Button Container");
            containerObject.transform.SetParent(playSelectionCanvas.transform);
            
            // Add vertical layout group
            VerticalLayoutGroup layoutGroup = containerObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 20;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false;
            
            // Position in center of screen
            RectTransform containerRect = containerObject.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.3f, 0.3f);
            containerRect.anchorMax = new Vector2(0.7f, 0.7f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            playButtonContainer = containerObject.transform;
        }
    }
    
    void LoadAvailablePlays()
    {
        // Add all available plays here
        availablePlays.Clear();
        availablePlays.Add(PlayBook.GetOutsidePlay());
        availablePlays.Add(PlayBook.GetMeshPlay());
        
        Debug.Log($"Loaded {availablePlays.Count} available plays");
    }
    
    void CreatePlayButtons()
    {
        // Clear existing buttons
        playButtons.Clear();
        
        foreach (PlayData play in availablePlays)
        {
            Button button = CreatePlayButton(play);
            playButtons.Add(button);
        }
    }
    
    Button CreatePlayButton(PlayData play)
    {
        // Create button
        GameObject buttonObject = new GameObject($"Play Button - {play.playName}");
        buttonObject.transform.SetParent(playButtonContainer);
        
        // Add button component
        Button button = buttonObject.AddComponent<Button>();
        
        // Add image for button background
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Set button size
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(400, 80);
        
        // Create button text
        GameObject textObject = new GameObject("Button Text");
        textObject.transform.SetParent(buttonObject.transform);
        
        Text buttonText = textObject.AddComponent<Text>();
        buttonText.text = $"{play.playName}\n{play.formation.formationName} - {play.playType}";
                    buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        
        // Position text in center of button
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Add button click handler
        button.onClick.AddListener(() => SelectPlay(play));
        
        Debug.Log($"Created button for play: {play.playName}");
        
        return button;
    }
    
    void HandleNavigationInput()
    {
        // Handle vertical navigation (W/S keys or left stick)
        float verticalInput = Input.GetAxis("Vertical");
        
        if (verticalInput > 0.5f) // Up
        {
            NavigateUp();
        }
        else if (verticalInput < -0.5f) // Down
        {
            NavigateDown();
        }
        
        // Handle discrete key presses for more responsive navigation
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            NavigateUp();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            NavigateDown();
        }
    }
    
    void HandleSelectionInput()
    {
        // Handle play selection (Tab, X button, or Enter)
        // Support both joystick-specific and general joystick button mappings
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Return) || 
            Input.GetButtonDown("Fire1") || Input.GetButtonDown("Submit") ||
            Input.GetKeyDown(KeyCode.Joystick1Button0) || // PS5 X button (Cross) - Joystick 1
            Input.GetKeyDown(KeyCode.JoystickButton0) ||  // PS5 X button (Cross) - Any joystick
            Input.GetKeyDown(KeyCode.Joystick1Button1) || // PS5 Circle button (alternative)
            Input.GetButtonDown("Jump"))  // Alternative mapping
        {
            Debug.Log("Play selection input detected!");
            SelectCurrentPlay();
        }
    }
    
    void NavigateUp()
    {
        if (availablePlays.Count == 0) return;
        
        currentSelectionIndex--;
        if (currentSelectionIndex < 0)
        {
            currentSelectionIndex = availablePlays.Count - 1;
        }
        
        UpdateSelection();
        nextInputTime = Time.time + inputCooldown;
    }
    
    void NavigateDown()
    {
        if (availablePlays.Count == 0) return;
        
        currentSelectionIndex++;
        if (currentSelectionIndex >= availablePlays.Count)
        {
            currentSelectionIndex = 0;
        }
        
        UpdateSelection();
        nextInputTime = Time.time + inputCooldown;
    }
    
    void UpdateSelection()
    {
        if (playButtons.Count == 0 || currentSelectionIndex < 0 || currentSelectionIndex >= playButtons.Count)
            return;
        
        // Update button colors to show selection
        for (int i = 0; i < playButtons.Count; i++)
        {
            Image buttonImage = playButtons[i].GetComponent<Image>();
            if (i == currentSelectionIndex)
            {
                buttonImage.color = new Color(0.8f, 0.8f, 0.2f, 0.8f); // Highlighted
            }
            else
            {
                buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Normal
            }
        }
    }
    
    void SelectCurrentPlay()
    {
        if (currentSelectionIndex >= 0 && currentSelectionIndex < availablePlays.Count)
        {
            SelectPlay(availablePlays[currentSelectionIndex]);
        }
    }
    
    public void SelectPlay(PlayData play)
    {
        selectedPlay = play;
        playSelected = true;
        
        Debug.Log($"Play selected: {play.playName} (Formation: {play.formation.formationName}, Type: {play.playType})");
        
        // Update UI
        selectedPlayText.text = $"Selected: {play.playName}";
        selectedPlayText.gameObject.SetActive(true);
        
        // Hide the canvas
        playSelectionCanvas.gameObject.SetActive(false);
        
        // Trigger event
        OnPlaySelected?.Invoke(play);
    }
    
    public PlayData GetSelectedPlay()
    {
        return selectedPlay;
    }
    
    public bool IsPlaySelected()
    {
        return playSelected;
    }
    
    public void ShowPlaySelection()
    {
        playSelectionCanvas.gameObject.SetActive(true);
        playSelected = false;
        selectedPlay = null;
        selectedPlayText.gameObject.SetActive(false);
        
        // Reset selection to first play
        currentSelectionIndex = 0;
        UpdateSelection();
    }
    
    public void HidePlaySelection()
    {
        playSelectionCanvas.gameObject.SetActive(false);
    }
} 