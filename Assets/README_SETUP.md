# 🏈 Football Game - Basic Setup Complete!

## 🚀 Quick Start Guide

### 1. Initial Setup
1. Open the `SampleScene` in Unity
2. Go to the top menu: **Football Game > Setup Scene**
3. Wait for the automatic setup to complete
4. Check the console for setup instructions

### 2. Testing Your Game
1. Press **Play** in Unity
2. Use **WASD** keys to move your player around the field
3. Hold **Left Shift** to run
4. Use **Tab** to switch between players (when you have multiple)

### 3. Adding More Players
1. Go to **Football Game > Add Additional Player**
2. Repeat to add more team members
3. Test player switching with **Tab**

## 🎮 Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD | Left Stick |
| Run | Hold Left Shift | Hold Right Trigger |
| Switch Player | Tab | Y Button (Xbox) |

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Player/
│   │   └── PlayerController.cs         # Player movement & input
│   ├── Camera/
│   │   └── CameraController.cs         # Camera following system
│   ├── Environment/
│   │   └── FootballField.cs            # Field generation
│   ├── Setup/
│   │   ├── SceneSetupGuide.cs          # Automatic scene setup
│   │   └── EditorSetupTool.cs          # Editor menu tools
│   └── GameManager.cs                  # Game coordination & player management
├── Resources/
│   └── FootballInput.inputactions      # Input System configuration
└── README_SETUP.md                     # This file
```

## 🔧 System Features

### ✅ Completed Features
- **Player Movement**: Smooth WASD/controller movement with running
- **Camera System**: Automatic camera following with smooth transitions
- **Team Management**: Player switching system ready for multiplayer
- **Field Generation**: Procedural football field with boundaries
- **Input System**: Full keyboard and gamepad support
- **Easy Setup**: One-click scene setup through editor menu

### 🎯 Architecture Highlights
- **Multiplayer Ready**: All systems designed for future online multiplayer
- **Modular Design**: Easy to extend with new features
- **Performance Focused**: Efficient movement using Rigidbody physics
- **Input System**: Modern Unity Input System implementation

## 🛠️ Editor Tools

Access these tools via the **Football Game** menu:

- **Setup Scene**: Creates the complete game scene
- **Add Additional Player**: Adds more team members
- **Show Controls**: Displays control reference
- **Reset Scene**: Cleans up all football objects

## 🔄 Team Switching System

The game includes a robust team switching system:

```csharp
// Switch to next player
GameManager.Instance.SwitchToNextPlayer();

// Switch to specific player
GameManager.Instance.SwitchToPlayer(playerIndex);

// Get current active player
PlayerController currentPlayer = GameManager.Instance.GetCurrentPlayer();
```

## 🌐 Multiplayer Preparation

The codebase is designed for future multiplayer implementation:

- **Player Authority**: Each player has `isLocalPlayer` flag
- **Network Ready**: Player IDs and team management in place
- **Mirror Compatible**: Structure ready for Mirror Networking
- **Scalable**: Supports up to 22 players (11 per team)

## 📦 Dependencies

Make sure you have these Unity packages installed:
- **Input System** (for player controls)
- **TextMeshPro** (for UI text)

## 🚀 Next Steps

1. **Test Basic Movement**: Walk around the field with WASD
2. **Add Team Members**: Use the editor tool to add more players
3. **Test Team Switching**: Use Tab to switch between players
4. **Implement Ball Mechanics**: Add ball physics and passing
5. **Add Multiplayer**: Integrate Mirror Networking
6. **Create Game Modes**: Add different game types and rules

## 🐛 Troubleshooting

### Player Won't Move
- Check that Input System package is installed
- Verify `FootballInput.inputactions` is in the Resources folder
- Ensure the player has a Rigidbody component

### Camera Not Following
- Make sure the Main Camera has a `CameraController` component
- Check that the GameManager has references to the camera and players

### Player Switching Not Working
- Verify multiple players exist in the scene
- Check that each player has a unique playerID
- Ensure only one player has `isLocalPlayer = true`

## 📚 Code Examples

### Adding a New Player Programmatically
```csharp
Vector3 spawnPosition = new Vector3(0, 1, 0);
GameManager.Instance.AddPlayer(spawnPosition, "New Player");
```

### Getting Field Bounds
```csharp
FootballField field = GameManager.Instance.GetField();
Bounds fieldBounds = field.GetFieldBounds();
```

### Camera Control
```csharp
CameraController camera = FindObjectOfType<CameraController>();
camera.SetTarget(newPlayerTransform);
```

## 🎉 You're Ready to Play!

Your basic football game is now set up and ready for testing. The foundation is solid and prepared for future enhancements like ball mechanics, AI players, and multiplayer networking.

Happy coding! 🏈 