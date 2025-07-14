# Football Game - Unity Project

A Unity football game featuring controllable players, play selection, and offensive gameplay mechanics.

## 🎮 Controls

### Keyboard
- **WASD** - Move player
- **Tab** - Snap ball / Pass ball (context-sensitive)
- **W/S** - Navigate play selection menu
- **Tab** - Select play from menu

### PS5 Controller
- **Left Stick** - Move player
- **X Button** - Snap ball / Pass ball (context-sensitive)
- **Left Stick Up/Down** - Navigate play selection menu
- **X Button** - Select play from menu

## 🏈 Gameplay Overview

### Pre-Play Phase
1. **Play Selection** - Choose from available offensive plays
2. **Formation Setup** - Players automatically position themselves
3. **Snap Count** - Press Tab/X to snap the ball and start the play

### Post-Play Phase
- **Run Plays** - Control the ball carrier after handoff
- **Pass Plays** - Control QB, then target receivers with directional input
- **Player Switching** - Switch between team players (when applicable)

## 📋 Available Plays

### "Outside" - Run Play
- **Formation**: Single Back
- **Concept**: RB runs outside through left gap
- **Key Players**: 
  - QB hands off to RB
  - RB runs to gap between TE and LT
  - WRs run straight downfield routes
- **User Control**: Automatically switches to RB after handoff

### "Mesh" - Pass Play
- **Formation**: Single Back
- **Concept**: Crossing routes with mesh concept
- **Key Players**:
  - QB drops back to pass
  - X & Y receivers run crossing routes (mesh)
  - W & Z receivers run corner routes upfield
  - RB stays back for pass protection
- **User Control**: Control QB, use WASD/Left Stick to target receivers

## 🎯 Passing System

### How to Pass
1. Select a **Pass** type play (like "Mesh")
2. Snap the ball with **Tab/X**
3. **Hold WASD/Left Stick** in the direction of your target receiver
4. Press **Tab/X** to throw the ball

### Smart Features
- **Automatic Leading** - Ball leads moving receivers
- **Distance Adjustment** - Throw force adjusts based on target distance
- **Best Target Selection** - System finds receiver closest to your input direction
- **Receiver Filtering** - Only targets eligible receivers (WR, TE, RB)

## 🏃 Player Movement

### AI-Controlled Routes
- Players automatically run their assigned routes
- **User Override** - Take control of any player by moving them
- **Route Visualization** - Yellow gizmos show planned routes (in Scene view)

### Manual Control
- **Smooth Movement** - Responsive player movement
- **Automatic Rotation** - Players face their movement direction
- **Ball Carrying** - Ball automatically follows the current carrier

## 🎮 Game Flow

1. **Game Start** → Play selection UI appears
2. **Select Play** → Choose between run and pass plays
3. **Pre-Play** → Players position themselves in formation
4. **Snap Ball** → Press Tab/X to start the play
5. **Execute Play** → 
   - **Run**: Control RB after handoff
   - **Pass**: Control QB, target receivers, throw ball
6. **User Control** → Take control of any player with movement input

## 🏆 Tips for Success

### Running Plays
- Wait for the handoff to complete before taking control
- Use the angle of the route to maximize yardage
- Players will continue their routes until you take control

### Passing Plays
- Study receiver routes before snapping
- Use the left stick/WASD to precisely target receivers
- The ball will automatically lead moving targets
- Corner routes (W/Z) are great for deeper passes
- Crossing routes (X/Y) are good for shorter, quick passes

### General Tips
- Players stay on their routes until you provide input
- The camera follows the currently controlled player
- Debug messages in the console show system feedback
- Yellow route visualization helps plan your strategy

## 🔧 Formation System

### Single Back Formation
- **QB** - Under center
- **RB** - Behind QB
- **WR1 (X)** - Split wide left
- **WR2 (Y)** - Split wide right  
- **TE1 (W)** - Left side tight end
- **TE2 (Z)** - Right side tight end
- **Linemen** - Standard offensive line positions

## 🎯 Future Features
- Additional formations (I-Formation, Shotgun, etc.)
- More play types (Screen, Draw, etc.)
- Defensive players and AI
- Online multiplayer support
- Team switching capabilities

---

**Have fun playing! The game focuses on authentic football strategy with intuitive controls.** 