using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider masterSlider;          // Min 0, Max 100
    public TMP_Text masterValueText;     // the "80%" text

    private const string MasterKey = "masterPercent"; // store 0..100

    private void Start()
    {
        // Load saved percent (0..100). Default = 80
        int savedPercent = PlayerPrefs.GetInt(MasterKey, 80);

        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(savedPercent);

        ApplyMasterFromSlider(); // apply + update label at start
    }

    // Hook THIS in Slider -> On Value Changed (Single)
    public void ApplyMasterFromSlider()
    {
        if (masterSlider == null) return;

        float percent = masterSlider.value; // 0..100
        float volume01 = Mathf.Clamp01(percent / 100f);

        // Apply volume
        AudioListener.volume = volume01;

        // Update text
        if (masterValueText != null)
            masterValueText.text = $"{Mathf.RoundToInt(percent)}%";

        // Save
        PlayerPrefs.SetInt(MasterKey, Mathf.RoundToInt(percent));
        PlayerPrefs.Save();
    }
}