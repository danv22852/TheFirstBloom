using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    public Transform pressurePlate;

    // This keeps track of how many valid objects are on the plate
    private int objectsOnPlate = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Rock") || other.CompareTag("Player"))
        {
            objectsOnPlate++;

            // Only activate the plate if this is the very first object to step on it
            if (objectsOnPlate == 1)
            {
                onActivate.Invoke();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Rock") || other.CompareTag("Player"))
        {
            objectsOnPlate--;

            // A safety net to ensure our counter never goes below zero
            if (objectsOnPlate < 0) 
            {
                objectsOnPlate = 0;
            }

            // Only deactivate the plate if ALL objects have stepped off
            if (objectsOnPlate == 0)
            {
                onDeactivate.Invoke();
            }
        }
    }
}