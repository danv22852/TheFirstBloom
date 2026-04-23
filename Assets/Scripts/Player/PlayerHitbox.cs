using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Check if the thing we just hit is tagged as an Enemy
        if (collision.CompareTag("Enemy"))
        {
            // 2. Try to find the EnemyEncounter script on the monster we hit
            EnemyEncounter encounter = collision.GetComponent<EnemyEncounter>();
            
            // (Sometimes the script is on the parent object, so let's check there just in case)
            if (encounter == null) encounter = collision.GetComponentInParent<EnemyEncounter>();

            // 3. If we found the encounter script, force the battle to start!
            if (encounter != null)
            {
                Debug.Log("Player struck the enemy! Initiating combat.");

                // Grab the player's transform to pass into your teammate's Engage function
                Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
                
                encounter.Engage(playerTransform);

                // Turn off the hitbox immediately so it doesn't trigger the battle 5 times in one frame!
                gameObject.SetActive(false); 
            }
        }
    }
}