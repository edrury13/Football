using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerFormationPosition
{
    public PlayerPosition playerPosition;
    public Vector3 position;
    
    public PlayerFormationPosition(PlayerPosition pos, Vector3 formationPos)
    {
        playerPosition = pos;
        position = formationPos;
    }
}

[System.Serializable]
public class FormationData
{
    public string formationName;
    public string description;
    public List<PlayerFormationPosition> playerPositions = new List<PlayerFormationPosition>();
    
    public FormationData(string name, string desc)
    {
        formationName = name;
        description = desc;
    }
    
    public Vector3 GetPositionForPlayer(PlayerPosition position)
    {
        foreach (var playerPos in playerPositions)
        {
            if (playerPos.playerPosition == position)
            {
                return playerPos.position;
            }
        }
        return Vector3.zero; // Default if not found
    }
}

public static class FormationBook
{
    public static FormationData GetSingleBackFormation()
    {
        FormationData formation = new FormationData("Single Back", "Standard single back offensive formation");
        
        // Offensive Line (Z=0 is line of scrimmage)
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.LT, new Vector3(-8f, 0f, 0f)));   // Left Tackle
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.LG, new Vector3(-4f, 0f, 0f)));   // Left Guard  
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.C, new Vector3(0f, 0f, 0f)));     // Center
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.RG, new Vector3(4f, 0f, 0f)));    // Right Guard
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.RT, new Vector3(8f, 0f, 0f)));    // Right Tackle
        
        // Tight Ends
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.TE1, new Vector3(-12f, 0f, 0f))); // Left Tight End
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.TE2, new Vector3(12f, 0f, 0f)));  // Right Tight End
        
        // Backfield (behind line of scrimmage)
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.QB, new Vector3(0f, 0f, -7f)));   // Quarterback
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.RB, new Vector3(0f, 0f, -12f)));  // Running Back
        
        // Wide Receivers
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.WR1, new Vector3(-20f, 0f, 0f))); // Left Wide Receiver
        formation.playerPositions.Add(new PlayerFormationPosition(PlayerPosition.WR2, new Vector3(20f, 0f, 0f)));  // Right Wide Receiver
        
        return formation;
    }
} 