using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool isPressed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PushableRock"))
        {
            isPressed = true;
            // You can trigger doors, events, etc.
            Debug.Log("Plate pressed!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PushableRock"))
        {
            isPressed = false;
            Debug.Log("Plate released!");
        }
    }
}