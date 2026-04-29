using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDataInitializer : MonoBehaviour
{
    public PlayerData playerData;

    private void Start()
    {
        playerData.floorName = SceneManager.GetActiveScene().name;
    }
}