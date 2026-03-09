using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDoor : MonoBehaviour
{
    public Tilemap doorTilemap; // Drag the specific "Door" Tilemap here

    // This runs when the Rock hits the plate
    public void OpenHole()
    {
        // This simply hides the tiles and turns off collision
        doorTilemap.gameObject.SetActive(false); 
    }

    // This runs when the Rock leaves the plate
    public void CloseHole()
    {
        doorTilemap.gameObject.SetActive(true);
    }
}