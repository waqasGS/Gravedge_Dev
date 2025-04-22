using UnityEngine;

// ScriptableObject to manage player stats and allow automatic saving
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public int health; // Player's health
    public int stamina; // Player's stamina
    public int experience; // Player's experience points

    public delegate void OnStatsChanged();
    public event OnStatsChanged StatsChanged; // Event triggered when stats change

    /// <summary>
    /// Loads player data from a saved PlayerData object.
    /// </summary>
    public void LoadData(PlayerData data)
    {
        health = data.health;
        stamina = data.stamina;
        experience = data.experience;
    }

    /// <summary>
    /// Updates a specific stat by a given value and triggers auto-save.
    /// </summary>
    public void UpdateStat(string statName, int value)
    {
        switch (statName.ToLower())
        {
            case "health":
                health = Mathf.Clamp(health + value, 0, 100); // Prevents negative health
                break;
            case "stamina":
                stamina = Mathf.Clamp(stamina + value, 0, 100);
                break;
            case "experience":
                experience += value;
                break;
            default:
                Debug.LogWarning("Invalid stat name: " + statName);
                return;
        }

        StatsChanged?.Invoke(); // Trigger event for auto-saving
    }
}

/// <summary>
/// Serializable class for saving and loading player stats data.
/// </summary>
[System.Serializable]
public class PlayerData
{
    public int health;
    public int stamina;
    public int experience;
}

/*
 * USAGE GUIDE:
 * 1. Create a PlayerStats asset in Unity (Assets > Create > Scriptable Objects > PlayerStats).
 * 2. Assign the PlayerStats asset to relevant game objects.
 * 3. Call `playerStats.UpdateStat("health", -10);` to reduce health by 10.
 *    Example Syntax:
 *    ```csharp
 *    playerStats.UpdateStat("stamina", 5); // Increase stamina by 5
 *    playerStats.UpdateStat("experience", 20); // Add 20 experience points
 *    ```
 * 4. Call `playerStats.LoadData(savedPlayerData);` to load saved stats.
 *    Example Syntax:
 *    ```csharp
 *    PlayerData savedData = new PlayerData { health = 80, stamina = 50, experience = 200 };
 *    playerStats.LoadData(savedData);
 *    ```
 * 5. Subscribe to `StatsChanged` to auto-save when stats update.
 *    Example Syntax:
 *    ```csharp
 *    playerStats.StatsChanged += SavePlayerStats;
 *    ```
 */