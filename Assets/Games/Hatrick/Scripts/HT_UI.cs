using UnityEngine;
using UnityEngine.SceneManagement;

public class HT_UI : MonoBehaviour
{
    GameObject[] pauseObjects, finishObjects;
    public AudioClip[] audioClips; // winlevel loose
    public AudioSource gamesound;
    public int winScore = 7;
    public bool isPaused;
    void Start()
    {
        isPaused = false;
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
        hidePaused();
        // Time.timeScale = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if (HatGameController.instance.isPlaying && !isPaused)
        {
            gameData.isGameLogging = true;
            hideFinished();
            hidePaused();

        }
        if (!HatGameController.instance.isPlaying)
        {
            gameData.isGameLogging = false;
            showFinished();
        }

        if ((Input.GetKeyDown(KeyCode.P)))
        {

            if (HatGameController.instance.isPlaying)
            {
                Debug.Log("pressed");
                gameData.isGameLogging = false;
                pauseControl();

            }
            else if (!HatGameController.instance.isPlaying)
            {
                HatGameController.instance.Restart();
                Debug.Log("Restarted");
            }
        }
    }


    public void Reload()
    {
        // Application.LoadLevel(Application.loadedLevel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
    void playAudio(int clipNumber)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = audioClips[clipNumber];
        audio.Play();
    }
    public void pauseControl()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;
            showPaused();
            isPaused = true;
        }
        else
        {
            isPaused = false;
            Time.timeScale = 1;
            hidePaused();
        }
    }

    public void showPaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
        }
    }

    public void hidePaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
        }
    }

    public void showFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(true);
        }
    }

    public void hideFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(false);
        }
    }
    // public void LoadLevel(string level)
    // {
    //     Application.LoadLevel(level);
    // }
}
