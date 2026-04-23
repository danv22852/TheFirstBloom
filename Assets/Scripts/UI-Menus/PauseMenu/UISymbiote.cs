using TMPro;
using UnityEngine;

public class UISymbiote : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI symbioteTab;
    [SerializeField] private GameObject symbiotePage;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if(GameManager.Instance.playerData.hasAlien)
        {
            symbioteTab.text = "Symbiote";
            symbiotePage.SetActive(true);
        }
        else
        {
            
            symbioteTab.text = "???";
            symbiotePage.SetActive(false);
        }
    }
}