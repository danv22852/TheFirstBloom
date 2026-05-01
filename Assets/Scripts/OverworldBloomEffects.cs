using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OverworldBloomEffects : MonoBehaviour
{
    [Header("Visual Effects")]
    public Image vignetteOverlay;
    public Color lowBloomColor = new Color(0.4f, 0f, 0.6f); 
    public Color mediumBloomColor = new Color(1f, 0f, 0.8f); 
    public Color highBloomColor = new Color(1f, 0f, 0f); 

    [Header("Pulse Settings")]
    public float lowPulseSpeed = 1f;
    public float mediumPulseSpeed = 2f;
    public float highPulseSpeed = 4f;
    public float maxAlpha = 0.3f; // How dark the vignette gets during the pulse

    private Coroutine currentPulseRoutine;
    private BloomState lastCheckedState = BloomState.Stable;

    private void Start()
    {
        Debug.Log("[BloomEffects] Script started! Setting up 1-second interval checks.");
        
        if (vignetteOverlay == null) 
        {
            Debug.LogError("[BloomEffects] FATAL: vignetteOverlay is completely missing from the Inspector!");
        }

        // Start checking the bloom state repeatedly
        InvokeRepeating(nameof(CheckBloomState), 0.1f, 1f);
    }

    private void CheckBloomState()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[BloomEffects] GameManager is missing. Cannot check bloom.");
            return;
        }

        if (GameManager.Instance.playerData == null)
        {
            Debug.LogWarning("[BloomEffects] PlayerData is missing inside GameManager.");
            return;
        }

        BloomState currentState = GameManager.Instance.playerData.currentBloomState;
        
        // This will print once per second so we can see EXACTLY what the script is reading
        Debug.Log($"[BloomEffects] TICK - Reading State: {currentState} | Last State Was: {lastCheckedState} | Raw Bloom: {GameManager.Instance.playerData.currentBloom}");

        // If the state changed, update our effects!
        if (currentState != lastCheckedState)
        {
            Debug.Log($"[BloomEffects] STATE CHANGE DETECTED! Moving from {lastCheckedState} to {currentState}");
            lastCheckedState = currentState;
            UpdateOverworldEffects(currentState);
        }
    }

    private void UpdateOverworldEffects(BloomState state)
    {
        // 1. Stop any current animations
        if (currentPulseRoutine != null)
        {
            Debug.Log("[BloomEffects] Stopping previous pulse animation.");
            StopCoroutine(currentPulseRoutine);
        }

        // 2. Start the new animation based on the threshold
        if (state == BloomState.Stable)
        {
            Debug.Log("[BloomEffects] State is STABLE. Clearing the screen overlay.");
            if (vignetteOverlay != null) vignetteOverlay.color = new Color(0, 0, 0, 0);
        }
        else if (state == BloomState.Low)
        {
            Debug.Log("[BloomEffects] Triggering LOW pulse.");
            currentPulseRoutine = StartCoroutine(PulseRoutine(lowBloomColor, lowPulseSpeed));
        }
        else if (state == BloomState.Medium)
        {
            Debug.Log("[BloomEffects] Triggering MEDIUM pulse.");
            currentPulseRoutine = StartCoroutine(PulseRoutine(mediumBloomColor, mediumPulseSpeed));
        }
        else if (state == BloomState.High) 
        {
            Debug.Log("[BloomEffects] Triggering HIGH pulse.");
            currentPulseRoutine = StartCoroutine(PulseRoutine(highBloomColor, highPulseSpeed));
        }
    }

    private IEnumerator PulseRoutine(Color baseColor, float speed)
    {
        if (vignetteOverlay == null) 
        {
            Debug.LogError("[BloomEffects] Tried to start pulsing, but vignetteOverlay is missing!");
            yield break;
        }

        Debug.Log($"[BloomEffects] Pulse Loop has successfully started with Color: {baseColor} and Speed: {speed}");

        while (true)
        {
            // Use a Sine wave to smoothly loop between 0 and maxAlpha
            // Mathf.Sin goes from -1 to 1, so we adjust it to go from 0 to 1
            float pulseMath = (Mathf.Sin(Time.time * speed) + 1f) / 2f; 
            
            baseColor.a = pulseMath * maxAlpha;
            vignetteOverlay.color = baseColor;

            // Note: I am NOT putting a Debug.Log here because it would print 60 times a second and crash Unity!

            yield return null; // Wait for next frame
        }
    }
}