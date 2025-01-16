//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class HT_UI : MonoBehaviour
//{
//    GameObject[] pauseObjects, finishObjects;
//    public AudioClip[] audioClips; // winlevel loose
//    public AudioSource gamesound;
//    public int winScore = 7;
//    public bool isPaused;
//    void Start()
//    {
//        isPaused = false;
//        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
//        finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
//        hidePaused();
//        // Time.timeScale = 0;

//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (HatGameController.instance.isPlaying && !isPaused)
//        {
//            gameData.isGameLogging = true;
//            hideFinished();
//            hidePaused();

//        }
//        if (!HatGameController.instance.isPlaying)
//        {
//            gameData.isGameLogging = false;
//            showFinished();
//        }

//        if ((Input.GetKeyDown(KeyCode.P)))
//        {

//            if (HatGameController.instance.isPlaying)
//            {
//                Debug.Log("pressed");
//                gameData.isGameLogging = false;
//                pauseControl();

//            }
//            else if (!HatGameController.instance.isPlaying)
//            {
//                HatGameController.instance.Restart();
//                Debug.Log("Restarted");
//            }
//        }
//    }


//    public void Reload()
//    {
//        // Application.LoadLevel(Application.loadedLevel);
//        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
//    }
//    void playAudio(int clipNumber)
//    {
//        AudioSource audio = GetComponent<AudioSource>();
//        audio.clip = audioClips[clipNumber];
//        audio.Play();
//    }
//    public void pauseControl()
//    {
//        if (Time.timeScale == 1)
//        {
//            Time.timeScale = 0;
//            showPaused();
//            isPaused = true;
//        }
//        else
//        {
//            isPaused = false;
//            Time.timeScale = 1;
//            hidePaused();
//        }
//    }

//    public void showPaused()
//    {
//        foreach (GameObject g in pauseObjects)
//        {
//            g.SetActive(true);
//        }
//    }

//    public void hidePaused()
//    {
//        foreach (GameObject g in pauseObjects)
//        {
//            g.SetActive(false);
//        }
//    }

//    public void showFinished()
//    {
//        foreach (GameObject g in finishObjects)
//        {
//            g.SetActive(true);
//        }
//    }

//    public void hideFinished()
//    {
//        foreach (GameObject g in finishObjects)
//        {
//            g.SetActive(false);
//        }
//    }
//    // public void LoadLevel(string level)
//    // {
//    //     Application.LoadLevel(level);
//    // }
//}


using UnityEngine;
using UnityEngine.SceneManagement;

public class HT_UI : MonoBehaviour
{
    private GameObject[] pauseObjects, finishObjects;
    public AudioClip[] audioClips; // win, level complete, loose
    public AudioSource gameSound;
    public int winScore = 7;
    private bool isPaused;

    void Start()
    {
        isPaused = false;
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
        HidePaused();
        HideFinished();
    }

    void Update()
    {
        HandleGameState();
        HandleInput();
    }

    private void HandleGameState()
    {
        if (HatGameController.instance != null)
        {
            if (HatGameController.instance.IsPlaying && !isPaused)
            {
                gameData.isGameLogging = true;
                HideFinished();
                HidePaused();
            }
            else if (!HatGameController.instance.IsPlaying)
            {
                gameData.isGameLogging = false;
                ShowFinished();
            }
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (HatGameController.instance != null && HatGameController.instance.IsPlaying)
            {
                Debug.Log("Game paused via P key.");
                gameData.isGameLogging = false;
                TogglePause();
            }
            else if (HatGameController.instance != null && !HatGameController.instance.IsPlaying)
            {
                HatGameController.instance.RestartGame();
                Debug.Log("Game restarted via P key.");
            }
        }
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void PlayAudio(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < audioClips.Length && gameSound != null)
        {
            gameSound.clip = audioClips[clipIndex];
            gameSound.Play();
        }
        else
        {
            Debug.LogWarning("Invalid audio clip index or missing AudioSource component.");
        }
    }

    public void TogglePause()
    {
        if (Time.timeScale == 1)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        ShowPaused();
        isPaused = true;

        if (HatGameController.instance != null)
        {
            HatGameController.instance.PauseGame();
        }
    }

    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        HidePaused();

        if (HatGameController.instance != null)
        {
            HatGameController.instance.ResumeGame();
        }
    }

    public void ShowPaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
        }
    }

    public void HidePaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
        }
    }

    public void ShowFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(true);
        }
    }

    public void HideFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(false);
        }
    }
}
