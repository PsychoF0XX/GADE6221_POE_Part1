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
    [SerializeField] private float speedBoostMultiplier = 1.5f;

    [Header("HUD (optional)")]
    [SerializeField] private GameObject pickupHUDPanel;
    [SerializeField] private TMP_Text pickupLabel;
    [SerializeField] private Image pickupTimerBar;   // set Image type to Filled

    public bool IsShieldActive { get; private set; }
    public bool IsMagnetActive { get; private set; }

    private Coroutine activeCoroutine;
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

        pickupHUDPanel?.SetActive(false);
    }

    public void ActivatePickup(PickupType type)
    {
        // Instant pickups — apply immediately, don't cancel any active timed effect
        if (type == PickupType.Health)
        {
            GameManager.Instance?.AddLife();
            ShowFlash("HEALTH +1");
            return;
        }
        if (type == PickupType.Score)
        {
            GameManager.Instance?.AddScore(5);
            ShowFlash("SCORE +5");
            return;
        }

        // Timed pickups — cancel any running timed effect first
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            ResetAllEffects();
        }

        pickupHUDPanel?.SetActive(true);

        switch (type)
        {
            case PickupType.SpeedBoost:
                if (pickupLabel != null) pickupLabel.text = "SPEED BOOST";
                activeCoroutine = StartCoroutine(TimedEffect(speedBoostDuration, ApplySpeedBoost, RemoveSpeedBoost));
                break;

            case PickupType.Shield:
                if (pickupLabel != null) pickupLabel.text = "SHIELD";
                activeCoroutine = StartCoroutine(TimedEffect(shieldDuration, ApplyShield, RemoveShield));
                break;

            case PickupType.Magnet:
                if (pickupLabel != null) pickupLabel.text = "MAGNET";
                activeCoroutine = StartCoroutine(TimedEffect(magnetDuration, ApplyMagnet, RemoveMagnet));
                break;
        }
    }

    private void ShowFlash(string message)
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashLabel(message));
    }

    private IEnumerator FlashLabel(string message)
    {
        bool panelWasActive = pickupHUDPanel != null && pickupHUDPanel.activeSelf;
        string previousLabel = pickupLabel != null ? pickupLabel.text : "";

        if (pickupLabel != null) pickupLabel.text = message;
        if (!panelWasActive)
        {
            if (pickupTimerBar != null) pickupTimerBar.fillAmount = 0f;
            pickupHUDPanel?.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        if (panelWasActive)
            if (pickupLabel != null) pickupLabel.text = previousLabel;
        else
            pickupHUDPanel?.SetActive(false);

        flashCoroutine = null;
    }

    private IEnumerator TimedEffect(float duration, System.Action onActivate, System.Action onDeactivate)
    {
        onActivate?.Invoke();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (pickupTimerBar != null)
                pickupTimerBar.fillAmount = 1f - (elapsed / duration);
            yield return null;
        }

        onDeactivate?.Invoke();
        pickupHUDPanel?.SetActive(false);
        activeCoroutine = null;
    }

    private void ApplySpeedBoost() => player?.SetSpeedMultiplier(speedBoostMultiplier);
    private void RemoveSpeedBoost() => player?.ResetSpeed();

    private void ApplyShield() => IsShieldActive = true;
    private void RemoveShield() => IsShieldActive = false;

    private void ApplyMagnet() => IsMagnetActive = true;
    private void RemoveMagnet() => IsMagnetActive = false;

    private void ResetAllEffects()
    {
        RemoveSpeedBoost();
        RemoveShield();
        RemoveMagnet();
    }
}
