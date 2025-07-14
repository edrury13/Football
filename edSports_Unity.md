
# edSports Development Notes (Unity Edition)

## 🏗️ Project Overview

**Game Type:** 3D Football (American) Game  
**Engine:** Unity 2023 LTS  
**Platform:** Desktop  
**Key Features:**
- Online Multiplayer (Mirror)
- Controller Support (Unity Input System)
- Persistent User Teams
- Player Trading Market
- Custom Leagues & Tournaments

---

## 📁 Suggested Unity Project Structure

```
Assets/
├── Scenes/
│   └── Main.unity
├── Scripts/
│   ├── Network/
│   │   ├── NetworkManagerFootball.cs
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── BallController.cs
│   │   ├── PassSystem.cs
│   ├── UI/
│   │   ├── UIManager.cs
├── Prefabs/
│   ├── Player.prefab
│   ├── Ball.prefab
│   ├── Field.prefab
├── InputActions/
│   └── FootballInput.inputactions
```

---

## ✅ Feature Checklist

### 🎮 Core Mechanics
- [x] Player switching
- [x] Ball passing / throwing
- [x] Handoff logic (proximity-based)
- [x] Movement via Input System

### 🕹️ Controller Support
- Unity Input System for mapping actions
- Supports both keyboard and gamepads

### 🌐 Multiplayer (Mirror)
- Mirror Networking for syncing players, ball, game state
- Authority model for player input
- Server-based match management

### 💾 Persistent Data
- Save/Load using JSON, ScriptableObjects, or cloud services
- Player stat progression, aging, retirement

### 💰 Trading & Market
- In-game currency for trading players
- Backend service or local emulation for trade offers
- Validation and cooldowns for fair trades

### 🏆 Leagues & Tournaments
- League manager UI and logic
- Matchmaking and brackets
- Score and ranking tracking

---

## 🧠 AI Considerations

### Behavior
- Finite State Machine or Behavior Trees per role
- Assign roles dynamically: QB, RB, WR, etc.
- Use NavMesh or custom movement for defenders/receivers

### Awareness
- Track ball, zone coverage, proximity to targets
- Assign priorities (tackle, intercept, block)

---

## ⚙️ Input & UI

### Input System
- Input Actions for `Move`, `Pass`, `Handoff`, `SwitchPlayer`
- Support for both keyboard and controller bindings

### UI
- Main Menu, Team Management, Marketplace, League Browser
- In-game HUD: timer, score, possession, indicators

---

## 📚 Learning Resources

### General Unity
- [Unity Learn](https://learn.unity.com/)
- [Brackeys YouTube](https://www.youtube.com/user/Brackeys)
- [Jason Weimann YouTube](https://www.youtube.com/c/JasonWeimann)
- [GameDev.tv Courses (Udemy)](https://www.gamedev.tv/)

### Multiplayer
- [Mirror Documentation](https://mirror-networking.gitbook.io/docs/)
- [Photon Engine Docs](https://doc.photonengine.com/en-us/pun/current/getting-started/pun-intro)
- [Unity Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/about/index.html)

### Input & UI
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html)
- [Unity UI Toolkit](https://learn.unity.com/tutorial/introduction-to-ui-toolkit)

---

## 🛠️ Asset Resources

- [Unity Asset Store](https://assetstore.unity.com/)
- [Mixamo](https://www.mixamo.com/)
- [Kenney.nl Assets](https://kenney.nl/assets)

---

## 🧪 Suggested Development Order

1. Player movement and camera
2. Ball passing and handoff
3. Basic multiplayer with Mirror
4. AI opponent players and roles
5. Team save/load and player stats
6. Marketplace and currency
7. Leagues and brackets
8. UI/UX polish and settings

---

## 📦 Build & Export

- Export for desktop via Build Settings
- Profile performance using Unity Profiler
- Consider Addressables for dynamic content
