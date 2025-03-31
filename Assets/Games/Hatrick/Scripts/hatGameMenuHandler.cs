using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class hatGameMenuHandler : MonoBehaviour
{

    private string nextScene = "HatrickGame";
    private string prevScene = "choosegame";
    private bool isPressed = false;
    void Start()
    {

        PlutoComm.OnButtonReleased += onPlutoButtonReleased;

    }


    void Update()
    {
        if (isPressed)
        { 
            SceneManager.LoadScene(nextScene);
            isPressed = false;
        }
    }



    public void gameScene()
    {
        SceneManager.LoadScene(nextScene);
    }

    public void gameMenuScene() 
    { 
        SceneManager.LoadScene(prevScene);
    }

    private void onPlutoButtonReleased()
    {
        isPressed = true;
    }

    private void OnDestroy()
    {
        PlutoComm.OnButtonReleased -= onPlutoButtonReleased;
    }
}
