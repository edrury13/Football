using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerRoute
{
    public PlayerPosition playerPosition;
    public Vector3[] waypoints;
    public float speed = 5f;
    public float delayBeforeStart = 0f;
    public bool isHandoffTarget = false;
    public bool isHandoffGiver = false;
    public float handoffTiming = 1f; // When to handoff (in seconds after snap)
    
    public PlayerRoute(PlayerPosition pos, Vector3[] points, float spd = 5f, float delay = 0f)
    {
        playerPosition = pos;
        waypoints = points;
        speed = spd;
        delayBeforeStart = delay;
    }
}

[System.Serializable]
public enum PlayType
{
    Run,
    Pass
}

[System.Serializable]
public class PlayData
{
    public string playName;
    public string description;
    public FormationData formation;
    public PlayType playType;
    public List<PlayerRoute> playerRoutes = new List<PlayerRoute>();
    public float playDuration = 10f;
    
    public PlayData(string name, string desc, FormationData formationData, PlayType type)
    {
        playName = name;
        description = desc;
        formation = formationData;
        playType = type;
    }
    
    public PlayerRoute GetRouteForPosition(PlayerPosition position)
    {
        foreach (var route in playerRoutes)
        {
            if (route.playerPosition == position)
            {
                return route;
            }
        }
        return null;
    }
    
    public bool IsRunPlay()
    {
        return playType == PlayType.Run;
    }
    
    public bool IsPassPlay()
    {
        return playType == PlayType.Pass;
    }
}

public static class PlayBook
{
    public static PlayData GetOutsidePlay()
    {
        // Get the Single Back formation
        FormationData singleBack = FormationBook.GetSingleBackFormation();
        
        // Create the "Outside" run play
        PlayData play = new PlayData("Outside", "RB runs outside through left gap", singleBack, PlayType.Run);
        
        // QB Route: Move backwards and left, handoff to RB
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.QB,
            new Vector3[] { 
                new Vector3(-2f, 0f, -2f),   // Back and left for handoff
                new Vector3(-4f, 0f, -4f)    // Continue back after handoff
            },
            4f, 0f
        ) { isHandoffGiver = true, handoffTiming = 1.2f });
        
        // RB Route: Run to gap between TE1 and LT, then continue at same angle
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.RB,
            new Vector3[] { 
                new Vector3(-8f, 0f, 4f),   // Move to gap
                new Vector3(-20f, 0f, 10f), // Continue at same angle
                new Vector3(-32f, 0f, 16f)  // Keep running at angle
            },
            6f, 0f
        ) { isHandoffTarget = true });
        
        // WR1 Route: Run straight downfield
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.WR1,
            new Vector3[] { 
                new Vector3(-20f, 0f, 10f),  // Run downfield
                new Vector3(-20f, 0f, 30f)   // Continue deep
            },
            7f, 0f
        ));
        
        // WR2 Route: Run straight downfield
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.WR2,
            new Vector3[] { 
                new Vector3(20f, 0f, 10f),   // Run downfield
                new Vector3(20f, 0f, 30f)    // Continue deep
            },
            7f, 0f
        ));
        
        // TE1 Route: Stay in place to block (no route = no AI movement)
        // TE2 Route: Stay in place to block (no route = no AI movement)
        
        return play;
    }
    
    public static PlayData GetMeshPlay()
    {
        // Get the Single Back formation
        FormationData singleBack = FormationBook.GetSingleBackFormation();
        
        // Create the "Mesh" pass play
        PlayData play = new PlayData("Mesh", "Crossing routes with mesh concept", singleBack, PlayType.Pass);
        
        // QB Route: Drop back for passing
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.QB,
            new Vector3[] { 
                new Vector3(0f, 0f, -3f),   // Drop back to pocket
                new Vector3(0f, 0f, -4f)    // Hold in pocket
            },
            3f, 0f
        ));
        
        // RB Route: Stay back for pass protection (minimal route)
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.RB,
            new Vector3[] { 
                new Vector3(-2f, 0f, -2f),   // Move slightly back and left
                new Vector3(-2f, 0f, -2f)    // Hold position for blocking
            },
            3f, 0f
        ));
        
        // WR1 (X) Route: Crossing route from left to right
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.WR1,
            new Vector3[] { 
                new Vector3(-10f, 0f, 8f),   // Initial movement
                new Vector3(5f, 0f, 12f),    // Cross to right side
                new Vector3(15f, 0f, 15f),   // Continue crossing
                new Vector3(25f, 0f, 18f),   // Keep going same angle
                new Vector3(35f, 0f, 21f)    // Continue even further
            },
            7f, 0f
        ));
        
        // WR2 (Y) Route: Crossing route from right to left
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.WR2,
            new Vector3[] { 
                new Vector3(10f, 0f, 8f),    // Initial movement
                new Vector3(-5f, 0f, 12f),   // Cross to left side
                new Vector3(-15f, 0f, 15f),  // Continue crossing
                new Vector3(-25f, 0f, 18f),  // Keep going same angle
                new Vector3(-35f, 0f, 21f)   // Continue even further
            },
            7f, 0f
        ));
        
        // TE1 (W) Route: Corner route upfield and left
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.TE1,
            new Vector3[] { 
                new Vector3(-12f, 0f, 12f),  // Up and out
                new Vector3(-18f, 0f, 20f),  // Continue corner
                new Vector3(-22f, 0f, 30f),  // Deep corner
                new Vector3(-26f, 0f, 40f),  // Keep going same angle
                new Vector3(-30f, 0f, 50f)   // Continue even further
            },
            6f, 0f
        ));
        
        // TE2 (Z) Route: Corner route upfield and right
        play.playerRoutes.Add(new PlayerRoute(
            PlayerPosition.TE2,
            new Vector3[] { 
                new Vector3(12f, 0f, 12f),   // Up and out
                new Vector3(18f, 0f, 20f),   // Continue corner
                new Vector3(22f, 0f, 30f),   // Deep corner
                new Vector3(26f, 0f, 40f),   // Keep going same angle
                new Vector3(30f, 0f, 50f)    // Continue even further
            },
            6f, 0f
        ));
        
        // Linemen stay in place to block (no routes)
        
        return play;
    }
} 