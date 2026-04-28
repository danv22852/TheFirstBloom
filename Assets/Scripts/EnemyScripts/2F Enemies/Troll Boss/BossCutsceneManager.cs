using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossCutsceneManager : MonoBehaviour
{
    [Header("Cutscene Elements")]
    public Transform trollBossOverworld; 
    public EnemyData trollBossData;      
    public GameObject cmCamObject; 

    [Header("Cutscene Settings")]
    public float trollRunSpeed = 7f;
    public float cameraZoomSize = 3.5f; 
    public float cameraPanSpeed = 2.5f;
    public string combatSceneName = "CombatUI"; 

    private bool hasTriggered = false;

    private void Start()
    {
        // Check the PlayerData graveyard to see if the boss is already dead
        if (GameManager.Instance != null && GameManager.Instance.playerData != null && trollBossData != null)
        {
            if (GameManager.Instance.playerData.defeatedEnemies.Contains(trollBossData.enemyID))
            {
                if (trollBossOverworld != null) Destroy(trollBossOverworld.gameObject);
                Destroy(gameObject); 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayCutscene(collision.gameObject));
        }
    }

    private IEnumerator PlayCutscene(GameObject player)
    {
        // 1. FREEZE THE PLAYER COMPLETELY
        var pController = player.GetComponent<PlayerController>();
        if (pController != null) pController.enabled = false;

        var rb = player.GetComponent<Rigidbody2D>();
        // Use linearVelocity for Unity 6 compatibility
        if (rb != null) rb.linearVelocity = Vector2.zero; 

        var anim = player.GetComponent<Animator>(); 
        // Only set the parameter that exists in your Animator controller
        if (anim != null) anim.SetBool("isRunning", false);

        // 2. HIJACK THE CAMERA
        if (cmCamObject != null) cmCamObject.SetActive(false);

        Camera mainCam = Camera.main;
        float originalOrthoSize = mainCam.orthographicSize;
        Vector3 startCamPos = mainCam.transform.position;
        
        Vector3 targetCamPos = trollBossOverworld.position;
        targetCamPos.z = startCamPos.z; 

        // 3. PAN AND ZOOM TO THE BOSS
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraPanSpeed;
            mainCam.transform.position = Vector3.Lerp(startCamPos, targetCamPos, t);
            mainCam.orthographicSize = Mathf.Lerp(originalOrthoSize, cameraZoomSize, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.6f);

        // 4. THE TROLL CHARGES
        Vector3 targetBossPos = player.transform.position;
        
        if (trollBossOverworld.position.x > player.transform.position.x)
            targetBossPos.x += 1.2f; 
        else
            targetBossPos.x -= 1.2f; 

        while (Vector3.Distance(trollBossOverworld.position, targetBossPos) > 0.1f)
        {
            trollBossOverworld.position = Vector3.MoveTowards(trollBossOverworld.position, targetBossPos, trollRunSpeed * Time.deltaTime);

            Vector3 newCamPos = trollBossOverworld.position;
            newCamPos.z = startCamPos.z;
            mainCam.transform.position = newCamPos;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (cmCamObject != null) cmCamObject.SetActive(true);
        mainCam.orthographicSize = originalOrthoSize;

        // 5. TRIGGER THE BATTLE & SAVE DATA
        GameManager.pendingEnemyData = trollBossData;
        GameManager.playerFirstStrike = false; 
        GameManager.lastPlayerPosition = player.transform.position;
        GameManager.currentEnemyID = trollBossData.enemyID;
        GameManager.encounteredInstanceID = trollBossData.enemyID;

        SceneManager.LoadScene(combatSceneName);
    }
}