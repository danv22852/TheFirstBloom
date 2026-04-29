using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDataInitializer : MonoBehaviour
{
    public PlayerData playerData;

    private void Start()
    {
        playerData.floorName = SceneManager.GetActiveScene().name;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerData.floorName = scene.name;

        // ONLY reset temporary stuff (not enemies)
        // playerData.ResetSceneState();
    }
}