using UnityEngine;
using Unity.Cinemachine;

public class CameraUpdater : MonoBehaviour
{
    private CinemachineConfiner2D confiner;

    private void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();
        if (confiner == null)
            Debug.LogError("CinemachineConfiner2D not found on this GameObject!");
    }

    // Call this whenever the player enters a new room
    public void UpdateConfinerBounds(PolygonCollider2D newBounds)
    {
        if (confiner != null && newBounds != null)
        {
            confiner.BoundingShape2D = newBounds;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}