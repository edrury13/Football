# brainLift.md

## 🎮 Project Overview: Arcade-Style 3D American Football Game (Unity)

An arcade-style American football game with combat-style tackling, online multiplayer, persistent teams, and user-created leagues/tournaments.

---

## 🛠️ Game Features & Requirements

### ✅ Platform Targets

- Desktop (PC/Mac)
- Gamepad/Controller support

### ✅ Core Features

- Real-time online multiplayer (low latency)
- Persistent user-owned teams (progression, aging, trading)
- In-game marketplace (currency earned through matches)
- Leagues and tournaments created by users
- Arcade gameplay with exaggerated, combat-style tackling

---

## 🎮 Unity Setup & Version

### 🔷 Recommended Version

- **Unity 2023 LTS** for long-term stability
- Download from: [https://unity.com/releases/editor/whats-new/2023-lts](https://unity.com/releases/editor/whats-new/2023-lts)

---

## 📂 Project Structure (Unity)

```
Assets/
├── Art/
│   ├── Models/
│   ├── Textures/
│   └── Animations/
├── Audio/
├── Scripts/
│   ├── Gameplay/
│   ├── Networking/
│   └── UI/
├── Prefabs/
├── Scenes/
├── UI/
└── Resources/
```

---

## 🧠 Multiplayer: Mirror Setup Guide

### ✅ Step 1: Install Mirror

1. Open Unity → `Window > Package Manager`
2. Click `+` → `Add package from Git URL`
   - URL: `https://github.com/MirrorNetworking/Mirror.git`

### ✅ Step 2: Set Up Network Manager

- Create `NetworkManager` GameObject
- Add `NetworkManager` and `NetworkManagerHUD` components
- Assign player prefab to the `Player Prefab` field

### ✅ Step 3: Create Networked Player

```csharp
using Mirror;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour {
    public float moveSpeed = 5f;

    void Update() {
        if (!isLocalPlayer) return;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(x, 0, z) * moveSpeed * Time.deltaTime);
    }
}
```

- Add `NetworkIdentity` to player prefab
- Assign prefab to `NetworkManager`

### ✅ Step 4: Test Locally

- Run one instance in Unity editor (Host)
- Build and run separate .exe (Client)
- Test player movement & sync

### ✅ Bonus: Sync Player Names

```csharp
[SyncVar]
public string playerName;

public override void OnStartLocalPlayer() {
    CmdSetName("Player_" + Random.Range(1, 1000));
}

[Command]
void CmdSetName(string name) {
    playerName = name;
}
```

---

## 🌐 Hosting Options for Online Multiplayer

### 🖥️ Dedicated Server (Low Latency)

#### ✅ 1. Build Headless Server

- File → Build Settings → Target Linux/Windows
- Enable Headless Mode (Linux) or use CLI flags `-batchmode -nographics`
- Strip UI/client logic using `#if UNITY_SERVER`

#### ✅ 2. Deploy to VPS

Providers: DigitalOcean, AWS, Hetzner, etc.

```bash
chmod +x MyGameServer.x86_64
./MyGameServer.x86_64 -batchmode -nographics -port 7777
```

Ensure port 7777 is open in firewall

#### ✅ 3. Client Connects

```csharp
NetworkManager.singleton.networkAddress = "YOUR.SERVER.IP";
NetworkManager.singleton.StartClient();
```

---

### 🛰️ Relay Server Options

- **Unity Relay** (via Unity Netcode for GameObjects, not Mirror)
- **Steam Transport** (Mirror + Steamworks for NAT traversal)
- **PlayFab Multiplayer Servers**

---

### 🧪 Optimization Tips

- Use **KCP Transport** (Mirror’s default UDP-based)
- Avoid per-frame RPCs; batch or interpolate transforms
- Use **server-authoritative movement** with `[Command]` for actions
- Add lag compensation if needed for tackles or hits

---

## 🤼‍♂️ Combat-Style Tackling & Player Interaction

### 🔧 Animation vs Physics

| Type      | Use Case                |
| --------- | ----------------------- |
| Animation | Contact, predictability |
| Physics   | Hit reactions, ragdoll  |
| Hybrid ✅  | Best of both worlds     |

### 🔄 State Machine

```
Idle → Running → Tackling → HitReact → Down → GetUp
```

### 🎯 Tackle Detection

- Use trigger colliders for tackle zones
- Validate via angle, speed, and timing
- Use hitboxes: chest, legs, etc.

### 📦 Ball Logic

- Server-authoritative possession logic
- Fumbles, passes, pickups triggered by collisions
- Sync using `[Command]`, `[ClientRpc]`, and `[SyncVar]`

### 🧩 IK and Ragdoll

- Use Animation Rigging to enhance physicality
- Blend to ragdoll for dramatic hits

### 🧪 Debugging

- Visualize hitboxes with Gizmos
- Use logs and state debugging
- Test combos of movement, tackles, and possession

---

## 🎨 Asset Resources

### 🏈 Free/Low-Cost Player Models & Fields

- [Kenney.nl](https://kenney.nl)
- [Mixamo](https://www.mixamo.com)
- [OpenGameArt](https://opengameart.org)
- [Quaternius](https://quaternius.com)
- [Sketchfab](https://sketchfab.com) (Filter by free license)

### 🕺 Animations

- Free: Mixamo (tackles, runs, jumps)
- Paid: Synty Football Pack
- Custom blend combat moves

---

## 📚 Unity Learning Resources

| Resource | Description |
|--|--|
| [Unity Learn](https://learn.unity.com) | Official Unity tutorials |
| [Mirror Docs](https://mirror-networking.gitbook.io/docs/) | Mirror networking guide |
| [Mixamo](https://www.mixamo.com) | Free 3D animation and rigging |
| [Brackeys](https://www.youtube.com/brackeys) | Unity tutorials (YouTube) |
| [GameDevTV](https://www.udemy.com/user/gamedev-tv/) | Udemy Unity and networking courses |

---

## 🧠 Next Steps

- ✅ Basic player movement
- ✅ Multiplayer movement sync
- 🔄 Ball possession logic
- 🤼 Tackle animations, transitions, and collision
- 📡 Server/Relay deployment
- 🧠 Sync team data (stats, names) via `SyncVar` or DB
- 🌍 Matchmaking, tournaments, lobbies

https://chatgpt.com/share/6878335c-3350-8005-88f9-3af9d9bf1a4c
https://chatgpt.com/share/6878336f-f96c-8005-9e4b-94e632f7b1ae
https://youtu.be/2T0hDIoQKNc?si=gsH7vaCAhInWL9Qq
https://youtu.be/IlKaB1etrik?si=tVQC00kYY4WrHWRp
https://youtu.be/uXIPjMhS86w?si=D1PhvW9kP556yZg7 + the rest of his videos



