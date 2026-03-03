using UnityEngine;

public class SymbioteArm : MonoBehaviour
{
    public float damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy  = other.GetComponent<Enemy>();
        if (enemy != null)
        {      
            enemy.TakeDamage(damage);
        }
    }
}
