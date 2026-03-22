using UnityEngine;
using UnityEngine.Tilemaps;

public class SymbioteArm : MonoBehaviour
{
    public float damage = 1f;
    public TilemapBreaker tilemapBreaker;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Damage enemy
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {      
            enemy.TakeDamage(damage);
        }

        // Damage object-based breakable wall
        

        // Break tilemap walls
        Tilemap tilemap = other.GetComponent<Tilemap>();
        if (tilemap != null && tilemapBreaker != null)
        {
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            tilemapBreaker.BreakConnected(hitPoint);
        }
    }
}