using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public event Action OnObstaclePassed;

    public event Action<PickupType> OnPickupActivated;

    public event Action<int> OnBossSpawned;   // int = boss number (1 or 2)

    public event Action<int> OnBossBeaten;    // int = boss number (1 or 2)

    public event Action OnPlayerDied;


    public void RaiseObstaclePassed()
    {
        OnObstaclePassed?.Invoke();
    }

    public void RaisePickupActivated(PickupType type)
    {
        OnPickupActivated?.Invoke(type);
    }

    public void RaiseBossSpawned(int bossNumber)
    {
        OnBossSpawned?.Invoke(bossNumber);
    }

    public void RaiseBossBeaten(int bossNumber)
    {
        OnBossBeaten?.Invoke(bossNumber);
    }

    public void RaisePlayerDied()
    {
        OnPlayerDied?.Invoke();
    }
}

public enum PickupType
{
    SpeedBoost,
    Shield,
    Magnet
}