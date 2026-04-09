using UnityEngine;
using TMPro;

public class LoadoutPage : MonoBehaviour
{
    [Header("Core Slots")]
    public TextMeshProUGUI[] coreSlotLabels; // 5 elements

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerData == null) return;

        var cores = GameManager.Instance.playerData.equippedCores;

        for (int i = 0; i < coreSlotLabels.Length; i++)
        {
            if (i < cores.Count && cores[i] != null)
                coreSlotLabels[i].text = cores[i].coreName;
            else
                coreSlotLabels[i].text = "Empty";
        }
    }
}