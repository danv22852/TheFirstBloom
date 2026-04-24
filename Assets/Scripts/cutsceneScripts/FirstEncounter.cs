    using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using NUnit.Framework.Constraints;




public class FirstEncounter : MonoBehaviour
{
    [SerializeField] PolygonCollider2D targetTransition;
    [SerializeField] PolygonCollider2D leftTransition;

    [SerializeField] PolygonCollider2D rightTransition;

    [SerializeField] CutsceneEnemy theEnemy;

    private CinemachineConfiner2D confiner;


    
    private void Awake() { 
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (GameManager.Instance.playerData.finishedTutorial)
        {
            
            Destroy(gameObject); // No need for this cutscene if the tutorial is already done
        }
    }
        
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.Instance.playerData.finishedTutorial)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            pc.canMove = false;
            Debug.Log("1stEncounyer: " +   pc.canMove); // Debug statement to check movement state

             if (pc != null)
            {
                StartCoroutine(CutsceneStart(pc));
            }
         
        }
    }


    private void playTime()
    {
        Time.timeScale = 0f;
         Time.timeScale = 1f; 
         PlayerController pc = FindFirstObjectByType<PlayerController>();
            
         pc.canMove = true;
            
            Debug.Log("turning on the time" + Time.timeScale + pc.canMove);
    }

    private void UpdateCameraBoundary(PolygonCollider2D newBoundary) {
        if (confiner != null) {
            GameManager.currentMapBoundaryName = targetTransition.gameObject.name;
            confiner.BoundingShape2D = newBoundary;
            confiner.InvalidateBoundingShapeCache();
        }
    }

    IEnumerator CutsceneStart(PlayerController pc)
    {
        

        yield return new WaitForSeconds(2f);
        
        UpdateCameraBoundary(leftTransition);

        yield return new WaitForSeconds(2f);
        
        UpdateCameraBoundary(rightTransition);

        yield return new WaitForSeconds(2f);


        if(theEnemy != null) theEnemy.Appear();
        UpdateCameraBoundary(targetTransition);
        
        yield return new WaitForSeconds(1f);

        // 4. Enemy starts moving!
        if(theEnemy != null) theEnemy.StartChasing();

        yield return new WaitForSeconds(1.5f);


        

        // 5. Give control back to player
        
    



    }
}