using UnityEngine;
using UnityEngine.Tilemaps;

public class KeyLockedWall : MonoBehaviour
{
    public int requiredKeys = 4;

    [Header("Tilemap to remove")]
    public Tilemap lockedWallTilemap;

    private bool opened = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened) return;
        if (!other.CompareTag("Player")) return;

        PlayerData pd = GameManager.Instance.playerData;

        if (pd.keys >= requiredKeys)
        {
            OpenWall();
        }
        else
        {
            Debug.Log($"Need {requiredKeys} keys. You have {pd.keys}");
        }
    }

    void OpenWall()
    {
        opened = true;

        // Remove all tiles in that tilemap
        lockedWallTilemap.ClearAllTiles();

        // Disable collision so player can pass
        lockedWallTilemap.GetComponent<TilemapCollider2D>().enabled = false;

        Debug.Log("Wall opened!");
    }
}