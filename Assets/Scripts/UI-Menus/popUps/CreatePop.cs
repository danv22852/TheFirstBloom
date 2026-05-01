using UnityEngine;

public class CreatePop : MonoBehaviour
{
    private PopupManager popupManager;
    public string popupID;
    public string popupMessage;

    void Start()
    {
        popupManager = GetComponent<PopupManager>();

        if (popupManager == null) return;
        if (GameManager.Instance == null) return;

        // Only show if not seen before
        if (!GameManager.Instance.playerData.defeatedEnemies.Contains(popupID))
        {
            GameManager.Instance.playerData.defeatedEnemies.Add(popupID);
            popupManager.ShowPopup(popupMessage);
        }
    }
}