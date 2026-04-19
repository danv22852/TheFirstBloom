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
        // Start checking the bloom state repeatedly
        InvokeRepeating(nameof(CheckBloomState), 0.1f, 1f);
    }

    private void CheckBloomState()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerData == null) return;

        BloomState currentState = GameManager.Instance.playerData.currentBloomState;

        // If the state changed, update our effects!
        if (currentState != lastCheckedState)
        {
            lastCheckedState = currentState;
            UpdateOverworldEffects(currentState);
        }
    }

    private void UpdateOverworldEffects(BloomState state)
    {
        // 1. Stop any current animations
        if (currentPulseRoutine != null)
        {
            StopCoroutine(currentPulseRoutine);
        }

        // 2. Start the new animation based on the threshold
        if (state == BloomState.Stable)
        {
            // Clear the screen completely
            if (vignetteOverlay != null) vignetteOverlay.color = new Color(0, 0, 0, 0);
        }
        else if (state == BloomState.Low)
        {
            currentPulseRoutine = StartCoroutine(PulseRoutine(lowBloomColor, lowPulseSpeed));
        }
        else if (state == BloomState.Medium)
        {
            currentPulseRoutine = StartCoroutine(PulseRoutine(mediumBloomColor, mediumPulseSpeed));
        }
        else if (state == BloomState.High) // Changed from >= to exactly ==
        {
            currentPulseRoutine = StartCoroutine(PulseRoutine(highBloomColor, highPulseSpeed));
        }
        else if (state == BloomState.Total) // --- NEW: The "Full Bloom" lock ---
        {
            if (vignetteOverlay != null) 
            {
                // Lock the color completely solid! 
                // We add a tiny bit of extra opacity so it feels heavier than the normal pulse.
                Color doomColor = highBloomColor;
                doomColor.a = maxAlpha + 0.15f; 
                vignetteOverlay.color = doomColor;
            }
        }
    }

    private IEnumerator PulseRoutine(Color baseColor, float speed)
    {
        if (vignetteOverlay == null) yield break;

        while (true)
        {
            // Use a Sine wave to smoothly loop between 0 and maxAlpha
            // Mathf.Sin goes from -1 to 1, so we adjust it to go from 0 to 1
            float pulseMath = (Mathf.Sin(Time.time * speed) + 1f) / 2f; 
            
            baseColor.a = pulseMath * maxAlpha;
            vignetteOverlay.color = baseColor;

            yield return null; // Wait for next frame
        }
    }
}