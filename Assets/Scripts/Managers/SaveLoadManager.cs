using System.IO;  // For file handling (Read/Write JSON)
using UnityEngine; // For Unity-specific functionalities

public class SaveLoadManager : MonoBehaviour
{
    public PlayerStats playerStats; // Reference to the PlayerStats ScriptableObject
    private string filePath; // Path where the JSON file will be saved

    private void Awake()
    {
        // Define the save file location
        filePath = Path.Combine(Application.persistentDataPath, "playerStats.json");

        // Subscribe to the PlayerStats change event for auto-saving
        playerStats.StatsChanged += SavePlayerStats;
    }

    /// <summary>
    /// Saves PlayerStats data to a JSON file
    /// </summary>
    public void SavePlayerStats()
    {
        // Create a new PlayerData object and assign current player stats
        PlayerData data = new PlayerData
        {
            health = playerStats.health,
            stamina = playerStats.stamina,
            experience = playerStats.experience
        };

        // Convert data to JSON format
        string json = JsonUtility.ToJson(data, true);

        // Write JSON to file
        File.WriteAllText(filePath, json);
        Debug.Log("Data Saved Automatically!"); // Debug message for confirmation
    }

    /// <summary>
    /// Loads PlayerStats data from a JSON file
    /// </summary>
    public void LoadPlayerStats()
    {
        // Check if the save file exists
        if (File.Exists(filePath))
        {
            // Read JSON content from file
            string json = File.ReadAllText(filePath);

            // Convert JSON back to PlayerData object
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            // Update PlayerStats values with loaded data
            playerStats.LoadData(data);
            Debug.Log("Data Loaded Successfully!");
        }
        else
        {
            Debug.LogWarning("Save file not found!"); // Warning if no save file exists
        }
    }
}
