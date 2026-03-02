using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;

    void Start()
    {
        ActivateTab(0);
    }

    public void ActivateTab(int tabNum)
    {
        //0 = player, 1 = sym, 2 = settings

        for (int i = 0; i < tabImages.Length; i++)
        {
            pages[i].SetActive(false); 
            tabImages[i].color = Color.gray; 
        } 
        tabImages[tabNum].color = Color.white;
        pages[tabNum].SetActive(true);
    }
}
