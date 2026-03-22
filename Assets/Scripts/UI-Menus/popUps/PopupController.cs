using UnityEngine;

public class PopupController : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(true); // Ensures popup shows on scene load
        Time.timeScale = 0f;        // Optional: pauses game
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;        // Resume game
    }
}