using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class CoreOfferUI : MonoBehaviour
{
    [Header("Tiles")]
    public CoreOfferTile[] tiles;

    [Header("Description")]
    public TextMeshProUGUI skillDescription;
    private string defaultDescription = "Hover over a skill to see its description!";

    [Header("Swap Panel")]
    public GameObject swapPanel;
    public Button[] swapButtons;

    [Header("Buttons")]
    public Button declineButton;
    public Button cancelSwapButton;

    private CoreTemplate pendingCore;

    private void Start()
    {
        declineButton.onClick.AddListener(OnDecline);
        cancelSwapButton.onClick.AddListener(CancelSwap);
        swapPanel.SetActive(false);

        if (skillDescription != null)
            skillDescription.text = defaultDescription;

        var offer = CoreOfferManager.pendingOffer;
        if (offer == null) return;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (i < offer.Count)
            {
                tiles[i].gameObject.SetActive(true);
                tiles[i].Setup(offer[i], this);
            }
            else
            {
                tiles[i].gameObject.SetActive(false);
            }
        }
    }

    public void ShowDescription(CoreTemplate core)
    {
        if (skillDescription != null)
            skillDescription.text = core.coreName + "\n" + core.coreDescription + "\nBloom Cost: " + core.bloomCost;
    }

    public void HideDescription()
    {
        if (skillDescription != null)
            skillDescription.text = defaultDescription;
    }

    public void OnCoreSelected(CoreTemplate core)
    {
        var pd = GameManager.Instance.playerData;

        if (pd.equippedCores.Count < pd.maxCoreSlots)
        {
            pd.equippedCores.Add(core);
            ClosePopup();
        }
        else
        {
            pendingCore = core;
            OpenSwapPanel();
        }
    }

    private void OpenSwapPanel()
    {
        swapPanel.SetActive(true);
        var pd = GameManager.Instance.playerData;

        for (int i = 0; i < swapButtons.Length; i++)
        {
            if (i < pd.equippedCores.Count)
            {
                swapButtons[i].gameObject.SetActive(true);
                var label = swapButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = pd.equippedCores[i].coreName;

                int index = i;
                swapButtons[i].onClick.RemoveAllListeners();
                swapButtons[i].onClick.AddListener(() => OnSwapConfirmed(index));
            }
            else
            {
                swapButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnSwapConfirmed(int index)
    {
        var pd = GameManager.Instance.playerData;
        pd.equippedCores[index] = pendingCore;
        swapPanel.SetActive(false);
        ClosePopup();
    }

    public void CancelSwap()
    {
        pendingCore = null;
        swapPanel.SetActive(false);
    }

    private void OnDecline()
    {
        ClosePopup();
    }

    private void ClosePopup()
    {
        ReturnToSource();
    }

   private void ReturnToSource()
{
    string scene = GameManager.Instance.returnScene;

    if (string.IsNullOrEmpty(scene))
    {
        Debug.LogWarning("returnScene missing, defaulting to Overworld");
        scene = "Overworld";
    }

    // Close popup first (optional safety)
    SceneManager.UnloadSceneAsync("CorePopup");

    // IMPORTANT: actually move back to origin scene
    SceneManager.LoadScene(scene);
}
}