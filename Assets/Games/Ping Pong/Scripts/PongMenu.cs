using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.SearchService;

public class PongMenu : MonoBehaviour
{

    public Button playButton;
    public Button exitButton;
    void Start()
    {
        playButton.onClick.AddListener(LoadNextScene);
        exitButton.onClick.AddListener(onExitButtonClicked);
    }


    //loads inputted level
    public void onExitButtonClicked()
    {
        SceneManager.LoadScene("CHGAME");
    }
 
    void LoadNextScene()
    {
        SceneManager.LoadScene("PONGGAME");

    }
}
