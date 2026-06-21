using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Raised every time the player passes an obstacle
    public event Action OnObstaclePassed;

    // Raised when any timed pickup activates
    public event Action<PickupType> OnPickupActivated;

    // Raised when a boss spawns (bossNumber = 1, 2, or 3)
    public event Action<int> OnBossSpawned;

    // Raised when a boss is beaten (bossNumber = 1, 2, or 3)
    public event Action<int> OnBossBeaten;

    // Raised when the player dies
    public event Action OnPlayerDied;

    // Raised when a level loop completes
    public event Action OnLevelCompleted;

    public void RaiseObstaclePassed()      => OnObstaclePassed?.Invoke();
    public void RaisePickupActivated(PickupType type) => OnPickupActivated?.Invoke(type);
    public void RaiseBossSpawned(int num)  => OnBossSpawned?.Invoke(num);
    public void RaiseBossBeaten(int num)   => OnBossBeaten?.Invoke(num);
    public void RaisePlayerDied()          => OnPlayerDied?.Invoke();
    public void RaiseLevelCompleted()      => OnLevelCompleted?.Invoke();
}

public enum PickupType
{
    SpeedBoost,
    Shield,
    Magnet,
    Health,
    Score
}
