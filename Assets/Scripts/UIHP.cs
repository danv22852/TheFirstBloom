using TMPro;
using UnityEngine;

public class UIPlayerHP : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        hpText.text = "HP: " + GameManager.Instance.currentHP + "/" +
                      GameManager.Instance.maxHP;
    }
}