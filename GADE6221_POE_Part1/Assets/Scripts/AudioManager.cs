using System.Collections;
using UnityEngine;

// Singleton AudioManager — attach to a GameObject in your EndlessRunner scene.
// Assign the three audio clips in the Inspector.
// Survives scene loads so music continues into other scenes.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip damageSound;

    [Header("Volume")]
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float pickupVolume = 0.8f;
    [SerializeField] private float damageVolume = 0.8f;

    [Header("Damage Clip Trim")]
    [Tooltip("Only play this many seconds of the damage clip")]
    [SerializeField] private float damageSoundDuration = 0.6f;

    private AudioSource musicSource;
    private AudioSource pickupSource;
    private AudioSource damageSource;
    private Coroutine damageCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        pickupSource = gameObject.AddComponent<AudioSource>();
        pickupSource.loop = false;
        pickupSource.volume = pickupVolume;
        pickupSource.playOnAwake = false;

        damageSource = gameObject.AddComponent<AudioSource>();
        damageSource.loop = false;
        damageSource.volume = damageVolume;
        damageSource.playOnAwake = false;
    }

    private void Start()
    {
        PlayMusic();

        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnPickupActivated += OnPickupEvent;
            EventManager.Instance.OnPlayerDied      += PlayDamage;
        }
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnPickupActivated -= OnPickupEvent;
            EventManager.Instance.OnPlayerDied      -= PlayDamage;
        }
    }

    private void PlayMusic()
    {
        if (backgroundMusic == null) return;
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    private void OnPickupEvent(PickupType type) => PlayPickup();

    public void PlayPickup()
    {
        if (pickupSound == null || pickupSource == null) return;
        pickupSource.Stop();
        pickupSource.clip = pickupSound;
        pickupSource.Play();
    }

    public void PlayDamage()
    {
        if (damageSound == null || damageSource == null) return;
        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(PlayDamageForDuration());
    }

    private IEnumerator PlayDamageForDuration()
    {
        damageSource.Stop();
        damageSource.clip = damageSound;
        damageSource.Play();
        yield return new WaitForSeconds(damageSoundDuration);
        damageSource.Stop();
    }
}
