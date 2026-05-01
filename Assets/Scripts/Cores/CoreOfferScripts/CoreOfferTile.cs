using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CoreOfferTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("UI References")]
    public Image coreImage;
    public TextMeshProUGUI coreName;

    private CoreTemplate core;
    private CoreOfferUI offerUI;
    private Vector3 originalScale;

    public void Setup(CoreTemplate coreTemplate, CoreOfferUI ui)
    {
        core = coreTemplate;
        offerUI = ui;
        originalScale = transform.localScale;

        if (coreTemplate.coreSprite != null)
            coreImage.sprite = coreTemplate.coreSprite;

        coreName.text = coreTemplate.coreName;

        var button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnTileClicked);
    }

    private void OnTileClicked()
    {
        offerUI.OnCoreSelected(core);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideHighlight();
    }

    public void OnSelect(BaseEventData eventData)
    {
        ShowHighlight();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        HideHighlight();
    }

    private void ShowHighlight()
    {
        transform.localScale = originalScale * 1.05f;
        offerUI.ShowDescription(core);
    }

    private void HideHighlight()
    {
        transform.localScale = originalScale;
        offerUI.HideDescription();
    }
}