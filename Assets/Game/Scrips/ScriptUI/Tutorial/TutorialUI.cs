using UnityEngine;
using UnityEngine.UI; 

public class TutorialUI : MonoBehaviour
{
    [SerializeField] int currentPage = 0; 
    [SerializeField] GameObject[] pages;

    public Button previousPageButton;
    public Button nextPageButton;

    public GameObject allElements;

    private void UpdatePage()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
          
                pages[i].SetActive(i == currentPage);
            }
        }

    
        previousPageButton.interactable = currentPage == 0 ? false : true;

   
        nextPageButton.interactable = currentPage == pages.Length - 1 ? false : true;
    }

    public void NextPage()
    {
        currentPage += 1;
        UpdatePage();
    }

    public void PreviousPage()
    {
        currentPage -= 1;
        UpdatePage();
    }

    public void CloseTutorial()
    {
        foreach (GameObject page in pages)
        {
            if (page != null)
            {
                page.SetActive(false);
            }
        } 

        currentPage = 0;


allElements.SetActive(false);
    }

    public void OpenTutorial()
    {
UpdatePage();

allElements.SetActive(true);

    } 
}
