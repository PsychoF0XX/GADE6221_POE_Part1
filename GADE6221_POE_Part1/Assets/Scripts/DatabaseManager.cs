using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mono.Data.Sqlite;

// SQLite DatabaseManager — stores scores.db in Application.persistentDataPath.
// Works on any machine with no server required.
// Singleton — add to a GameObject in your Main Menu scene.
public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    private string DbPath => Path.Combine(Application.persistentDataPath, "scores.db");
    private string ConnectionString => "URI=file:" + DbPath;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitialiseDatabase();
    }

    private void InitialiseDatabase()
    {
        try
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "CREATE TABLE IF NOT EXISTS scores (" +
                        "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                        "player_name TEXT NOT NULL," +
                        "score INTEGER NOT NULL," +
                        "date_achieved DATETIME DEFAULT CURRENT_TIMESTAMP);";
                    cmd.ExecuteNonQuery();
                }
                Debug.Log("SQLite database ready at: " + DbPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SQLite init failed: " + e.Message);
        }
    }

    public void SaveScore(string playerName, int score)
    {
        try
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO scores (player_name, score) VALUES (@name, @score)";
                    cmd.Parameters.AddWithValue("@name", playerName);
                    cmd.Parameters.AddWithValue("@score", score);
                    cmd.ExecuteNonQuery();
                }
                Debug.Log($"Score saved: {playerName} - {score}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SQLite error saving score: " + e.Message);
        }
    }

    public List<ScoreEntry> GetTopScores(int limit = 5)
    {
        var results = new List<ScoreEntry>();
        try
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT player_name, score FROM scores ORDER BY score DESC LIMIT @limit";
                    cmd.Parameters.AddWithValue("@limit", limit);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new ScoreEntry
                            {
                                playerName = reader.GetString(0),
                                score      = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SQLite error getting scores: " + e.Message);
        }
        return results;
    }
}

public class ScoreEntry
{
    public string playerName;
    public int score;
}
