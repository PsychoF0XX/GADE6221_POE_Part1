using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [Header("MySQL Connection")]
    [SerializeField] private string host = "localhost";
    [SerializeField] private string port = "3306";
    [SerializeField] private string database = "gade6221";
    [SerializeField] private string user = "root";
    [SerializeField] private string password = "";

    private string ConnectionString =>
        $"server={host};port={port};user={user};password={password};database={database};";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        TestConnection();
    }

    private void TestConnection()
    {
        try
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                Debug.Log("MySQL connected successfully!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("MySQL connection failed: " + e.Message);
        }
    }

    // Call this when the player submits their name on the game over screen
    public void SaveScore(string playerName, int score)
    {
        try
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "INSERT INTO scores (player_name, score) VALUES (@name, @score)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", playerName);
                    cmd.Parameters.AddWithValue("@score", score);
                    cmd.ExecuteNonQuery();
                }
                Debug.Log($"Score saved: {playerName} - {score}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Database error saving score: " + e.Message);
        }
    }

    // Returns the top N scores, highest first
    public List<ScoreEntry> GetTopScores(int limit = 5)
    {
        var results = new List<ScoreEntry>();
        try
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "SELECT player_name, score FROM scores ORDER BY score DESC LIMIT @limit";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@limit", limit);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new ScoreEntry
                            {
                                playerName = reader.GetString("player_name"),
                                score = reader.GetInt32("score")
                            });
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Database error getting scores: " + e.Message);
        }
        return results;
    }
}

// Simple data container for a leaderboard row
public class ScoreEntry
{
    public string playerName;
    public int score;
}
