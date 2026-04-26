using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    private int rocksOnPlate = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Rock"))
        {
            rocksOnPlate++;

            if (rocksOnPlate == 1)
            {
                onActivate.Invoke();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Rock"))
        {
            rocksOnPlate--;

            if (rocksOnPlate < 0)
                rocksOnPlate = 0;

            if (rocksOnPlate == 0)
            {
                onDeactivate.Invoke();
            }
        }
    }
}