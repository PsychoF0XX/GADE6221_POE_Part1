using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PickupManager : MonoBehaviour
{
    public static PickupManager Instance { get; private set; }

    [Header("Pickup Durations")]
    [SerializeField] private float speedBoostDuration = 10f;
    [SerializeField] private float shieldDuration = 5f;
    [SerializeField] private float magnetDuration = 8f;

    [Header("Speed Boost")]
    [SerializeField] private float speedBoostMultiplier = 1.3f;

    // ── Dedicated per-pickup HUD rows ─────────────────────────────────────────
    // Create one row per pickup in your Canvas (label + Image fill bar).
    // Set each row inactive by default in the Inspector.
    [Header("HUD - Speed Boost Row")]
    [SerializeField] private GameObject speedBoostRow;
    [SerializeField] private Image speedBoostBar;

    [Header("HUD - Shield Row")]
    [SerializeField] private GameObject shieldRow;
    [SerializeField] private Image shieldBar;

    [Header("HUD - Magnet Row")]
    [SerializeField] private GameObject magnetRow;
    [SerializeField] private Image magnetBar;

    // ── Instant-pickup flash label (set inactive by default) ─────────────────
    [Header("Flash Label")]
    [SerializeField] private TMP_Text flashLabel;

    // ── Legacy single-panel (optional, still wired up if you haven't rebuilt HUD) ──
    [Header("Legacy Single HUD Panel (optional)")]
    [SerializeField] private GameObject pickupHUDPanel;
    [SerializeField] private TMP_Text pickupLabel;
    [SerializeField] private Image pickupTimerBar;

    public bool IsShieldActive { get; private set; }
    public bool IsMagnetActive { get; private set; }

    // Remaining seconds for each timed effect — written by coroutines, extended on stack
    private float speedBoostRemaining;
    private float shieldRemaining;
    private float magnetRemaining;

    private Coroutine speedBoostCoroutine;
    private Coroutine shieldCoroutine;
    private Coroutine magnetCoroutine;
    private Coroutine flashCoroutine;

    private PlayerController player;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.GetComponent<PlayerController>();

        SafeSetActive(speedBoostRow, false);
        SafeSetActive(shieldRow, false);
        SafeSetActive(magnetRow, false);
        pickupHUDPanel?.SetActive(false);
        flashLabel?.gameObject.SetActive(false);
    }

    public void ActivatePickup(PickupType type)
    {
        switch (type)
        {
            case PickupType.Health:
                GameManager.Instance?.AddLife();
                ShowFlash("HEALTH +1");
                return;

            case PickupType.Score:
                GameManager.Instance?.AddScore(5);
                ShowFlash("SCORE +5");
                return;

            case PickupType.SpeedBoost:
                EventManager.Instance?.RaisePickupActivated(PickupType.SpeedBoost);
                ShowFlash("SPEED BOOST");
                if (speedBoostCoroutine != null)
                {
                    // Stack: extend remaining time (capped at 2× base)
                    speedBoostRemaining = Mathf.Min(speedBoostRemaining + speedBoostDuration, speedBoostDuration * 2f);
                }
                else
                {
                    speedBoostRemaining = speedBoostDuration;
                    player?.SetSpeedMultiplier(speedBoostMultiplier);
                    SafeSetActive(speedBoostRow, true);
                    pickupHUDPanel?.SetActive(true);
                    if (pickupLabel != null) pickupLabel.text = "SPEED";
                    speedBoostCoroutine = StartCoroutine(SpeedBoostTick());
                }
                break;

            case PickupType.Shield:
                EventManager.Instance?.RaisePickupActivated(PickupType.Shield);
                ShowFlash("SHIELD");
                if (shieldCoroutine != null)
                {
                    shieldRemaining = Mathf.Min(shieldRemaining + shieldDuration, shieldDuration * 2f);
                }
                else
                {
                    shieldRemaining = shieldDuration;
                    IsShieldActive = true;
                    SafeSetActive(shieldRow, true);
                    pickupHUDPanel?.SetActive(true);
                    if (pickupLabel != null) pickupLabel.text = "SHIELD";
                    shieldCoroutine = StartCoroutine(ShieldTick());
                }
                break;

            case PickupType.Magnet:
                EventManager.Instance?.RaisePickupActivated(PickupType.Magnet);
                ShowFlash("MAGNET");
                if (magnetCoroutine != null)
                {
                    // Magnet stacks — each extra pickup meaningfully extends range time
                    magnetRemaining = Mathf.Min(magnetRemaining + magnetDuration, magnetDuration * 3f);
                }
                else
                {
                    magnetRemaining = magnetDuration;
                    IsMagnetActive = true;
                    SafeSetActive(magnetRow, true);
                    pickupHUDPanel?.SetActive(true);
                    if (pickupLabel != null) pickupLabel.text = "MAGNET";
                    magnetCoroutine = StartCoroutine(MagnetTick());
                }
                break;
        }
    }

    private IEnumerator SpeedBoostTick()
    {
        while (speedBoostRemaining > 0f)
        {
            speedBoostRemaining -= Time.deltaTime;
            float fill = Mathf.Clamp01(speedBoostRemaining / speedBoostDuration);
            if (speedBoostBar != null) speedBoostBar.fillAmount = fill;
            if (pickupTimerBar != null) pickupTimerBar.fillAmount = fill;
            yield return null;
        }
        player?.ResetSpeed();
        SafeSetActive(speedBoostRow, false);
        speedBoostCoroutine = null;
        HideLegacyPanelIfIdle();
    }

    private IEnumerator ShieldTick()
    {
        while (shieldRemaining > 0f)
        {
            shieldRemaining -= Time.deltaTime;
            float fill = Mathf.Clamp01(shieldRemaining / shieldDuration);
            if (shieldBar != null) shieldBar.fillAmount = fill;
            if (pickupTimerBar != null) pickupTimerBar.fillAmount = fill;
            yield return null;
        }
        IsShieldActive = false;
        SafeSetActive(shieldRow, false);
        shieldCoroutine = null;
        HideLegacyPanelIfIdle();
    }

    private IEnumerator MagnetTick()
    {
        while (magnetRemaining > 0f)
        {
            magnetRemaining -= Time.deltaTime;
            float fill = Mathf.Clamp01(magnetRemaining / magnetDuration);
            if (magnetBar != null) magnetBar.fillAmount = fill;
            if (pickupTimerBar != null) pickupTimerBar.fillAmount = fill;
            yield return null;
        }
        IsMagnetActive = false;
        SafeSetActive(magnetRow, false);
        magnetCoroutine = null;
        HideLegacyPanelIfIdle();
    }

    private void HideLegacyPanelIfIdle()
    {
        if (speedBoostCoroutine == null && shieldCoroutine == null && magnetCoroutine == null)
            pickupHUDPanel?.SetActive(false);
    }

    // Unity's == null handles both C# null and unassigned serialized fields
    private static void SafeSetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private void ShowFlash(string message)
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine(message));
    }

    private IEnumerator FlashRoutine(string message)
    {
        if (flashLabel != null)
        {
            flashLabel.text = message;
            flashLabel.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            flashLabel.gameObject.SetActive(false);
        }
        else if (pickupLabel != null && pickupHUDPanel != null)
        {
            bool wasActive = pickupHUDPanel.activeSelf;
            string prev = pickupLabel.text;
            pickupLabel.text = message;
            if (!wasActive) { if (pickupTimerBar != null) pickupTimerBar.fillAmount = 0f; pickupHUDPanel.SetActive(true); }
            yield return new WaitForSeconds(1.5f);
            if (wasActive) pickupLabel.text = prev;
            else pickupHUDPanel.SetActive(false);
        }
        flashCoroutine = null;
    }
}
