using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEditor.SceneManagement;

public class UIManagerPP : MonoBehaviour
{
    GameObject[] pauseObjects, finishObjects,hideGameObjects;
    public BoundController rightBound;
    public BoundController leftBound;
    public bool isFinished;
    public bool isPressed=false;
    public bool playerWon, enemyWon;
    public AudioClip[] audioClips; 
    public int win;
    private bool isPaused = false;
    // Use this for initialization
    void Start()
    {
        PlutoComm.OnButtonReleased += onPlutoButtonReleased;
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
        hideGameObjects = new GameObject[] { GameObject.FindGameObjectWithTag("Target"), GameObject.FindGameObjectWithTag("Player"), 
                                                GameObject.FindGameObjectWithTag("Enemy"), GameObject.FindGameObjectWithTag("hideOnFinish") };
        hideFinished();
    }

    // Update is called once per frame
    void Update()
    {
        CheckGameEndConditions();

        if (isFinished)
        {
            showFinished();
        }

        if ((Input.GetKeyDown(KeyCode.P) && !isFinished) || (isPressed && !isFinished))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                UnpauseGame();
            }

            isPressed = false;  // Reset isPressed after handling
        }



        
    }

    //private IEnumerator WaitAndStopGameLogging()
    //{
    //    yield return new WaitForSeconds(2.0f);
    //    AppData.isGameLogging = false;
    //}



    private void CheckGameEndConditions()
    {
        if (rightBound.enemyScore >= AppData.winningScore && !isFinished)
        {
            isFinished = true;
            enemyWon = true;
            playerWon = false;
            GameEnd();
        }
        else if (leftBound.playerScore >= AppData.winningScore && !isFinished)
        {
            isFinished = true;
            enemyWon = false;
            playerWon = true;
            GameEnd();
        }
    }

    private void GameEnd()
    {
        Camera.main.GetComponent<AudioSource>().Stop();
        playAudio(enemyWon ? 1 : 0);
        AppData.reps = 0;
        showFinished();
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        isPaused = true;
        showPaused();
        AppData.isGameLogging = false;
        Debug.Log("Game Paused");
    }

    private void UnpauseGame()
    {
        Time.timeScale = 1;
        isPaused = false;
        hidePaused();
        AppData.isGameLogging = true;
        Debug.Log("Game Unpaused");
    }


    private void onPlutoButtonReleased()
    {
        isPressed = true;
    }
        //Reloads the Level
  public void LoadScene(string sceneName)
    {
       SceneManager.LoadScene(sceneName);
    }

    //Reloads the Level
    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void playAudio(int clipNumber)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = audioClips[clipNumber];
        audio.Play();
    }
    
    //shows objects with ShowOnPause tag
    public void showPaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
      
        }

    }

    //hides objects with ShowOnPause tag
    public void hidePaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
           
        }

    }

    //shows objects with ShowOnFinish tag
    public void showFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(true);

        }
    

    }

    //hides objects with ShowOnFinish tag
    public void hideFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(false);
        }

    }

    private void OnDestroy()
    {
        if (ConnectToRobot.isPLUTO)
        {
            PlutoComm.OnButtonReleased -= onPlutoButtonReleased;
        }
    }



}
