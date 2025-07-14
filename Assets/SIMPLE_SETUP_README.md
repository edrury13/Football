# Simple Football Game Setup

This is a simplified football game setup that creates a full team in single back formation with player switching capabilities.

## Features

- **Full Team Formation**: 11 players in proper single back formation
- **Player Positions**: QB, RB, WR1, WR2, TE1, TE2, C, LG, RG, LT, RT
- **User Control**: Switch between QB, RB, WR, and TE positions only
- **Visual Indicators**: Red pulsing circles under controlled players
- **Position Names**: 3D text labels above each player
- **Formation Layout**: Realistic football formation positioning

## Quick Start

### Option 1: Editor Menu
1. Go to **Football Game > Setup > Create Team Formation**
2. This will create a full team with proper positioning

### Option 2: Manual Setup  
1. Create an empty GameObject in your scene
2. Add the `SimpleTestSetup` component
3. Click **"Create Team Formation"** in the Inspector
4. Or right-click the component and select **"Create Team Formation"**

## Controls

- **WASD** or **Arrow Keys**: Move the controlled player
- **Left Shift**: Run faster
- **Tab**: Switch between user-controllable players (QB → RB → WR1 → WR2 → TE1 → TE2)
- **Controller**: Left stick for movement, Square for run, Triangle for switch

## Player Positions

### User-Controllable (can switch between):
- **QB**: Quarterback - starts with control
- **RB**: Running Back
- **WR1**: Left Wide Receiver  
- **WR2**: Right Wide Receiver
- **TE1**: Left Tight End
- **TE2**: Right Tight End

### Non-Controllable (AI/Static):
- **C**: Center
- **LG**: Left Guard
- **RG**: Right Guard  
- **LT**: Left Tackle
- **RT**: Right Tackle

## Formation Layout

The formation is positioned with:
- **Line of Scrimmage** at Z=0
- **Backfield** at negative Z values
- **Receivers** spread wide on X axis
- **Offensive Line** centered and tight

## Camera System

- Camera follows the currently controlled player
- Smooth tracking with proper viewing angle
- Instant switching between players
- Position offset for optimal gameplay view

## Debugging

Each player shows debug information including:
- Player name and position
- World position coordinates
- Control state (local/remote)
- User controllable status

## Requirements

- Unity 2021.3 or later
- No external packages required
- Uses Unity's built-in Input Manager

## Notes

- All players are blue colored for team uniformity
- Only user-controllable players can be switched to
- Formation positions are based on realistic football layouts
- Non-controllable players are stationary (AI movement comes later)
- System is designed for future multiplayer expansion 