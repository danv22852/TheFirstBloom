using TMPro;
using UnityEngine;

public class UISymbiote : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI symbioteTab;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if(GameManager.Instance.playerData.hasAlien)
        {
            symbioteTab.text = "Symbiote";
        }
        else
        {
            symbioteTab.text = "???";
        }
    }
}