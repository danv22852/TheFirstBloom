using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapBreaker : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase breakableTile;
    public GameObject breakParticlePrefab;

    public void BreakConnected(Vector3 worldPosition)
    {
        Vector3Int startPos = tilemap.WorldToCell(worldPosition);

        if (tilemap.GetTile(startPos) != breakableTile)
            return;

        FloodFill(startPos);
    }

    void FloodFill(Vector3Int startPos)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue(startPos);
        visited.Add(startPos);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            if (tilemap.GetTile(current) != breakableTile)
                continue;

            Vector3 worldPos = tilemap.GetCellCenterWorld(current);

            if (breakParticlePrefab != null)
                Instantiate(breakParticlePrefab, worldPos, Quaternion.identity);

            tilemap.SetTile(current, null);

            EnqueueNeighbor(current + Vector3Int.up, queue, visited);
            EnqueueNeighbor(current + Vector3Int.down, queue, visited);
            EnqueueNeighbor(current + Vector3Int.left, queue, visited);
            EnqueueNeighbor(current + Vector3Int.right, queue, visited);
        }
    }

    void EnqueueNeighbor(Vector3Int pos, Queue<Vector3Int> queue, HashSet<Vector3Int> visited)
    {
        if (!visited.Contains(pos))
        {
            visited.Add(pos);
            queue.Enqueue(pos);
        }
    }
}