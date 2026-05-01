using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapper : MonoBehaviour
{
    public string sceneName;
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.playerData.floorName = sceneName;
            GameManager.Instance.returnScene = sceneName;
            SceneManager.LoadScene(sceneName);
        }
    }
}