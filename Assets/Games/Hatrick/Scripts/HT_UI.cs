using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HT_UI : MonoBehaviour
{
    GameObject[] pauseObjects, finishObjects;
    //public BoundController rightBound;
    //public BoundController leftBound;
    //public bool         HatGameController.instance.isPlaying;
    // public bool playerWon, enemyWon;
    public AudioClip[] audioClips; // winlevel loose
    public AudioSource gamesound;
    public int winScore = 7;
    public bool isPaused;

    // Use this for initialization
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
        //  Debug.Log(AppData.inputPressed());
        //  uses the p button to pause and unpause the game
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
        //  HatGameController.instance.isPlaying
        //Debug.Log(HatGameController.instance.isPlaying);

        //if (HatGameController.instance.score  >= winScore && HatGameController.instance.isPlaying)
        //{
        //            HatGameController.instance.isPlaying = true;
        //   // enemyWon = true;
        //    //Camera.main.GetComponent<AudioSource>().Stop();

        //    //layAudio(1);
        //   //playerWon = false;
        //}






        //if (Time.timeScale == 0 && HatGameController.instance.isPlaying)
        //{
        //    Debug.Log("here 12sd3");

        //    showPaused();

        //    //;            //searches through pauseObjects for PauseText
        //    //            foreach (GameObject g in pauseObjects)
        //    //            {

        //    //                //if (g.name == "PauseText")
        //    //                    //makes PauseText to Active
        //    //                    g.SetActive(true);
        //    //            }
        //}
        //else if(Time.timeScale != 0 && HatGameController.instance.isPlaying && !isPaused)
        //{
        //    Debug.Log("here 435");
        //    hidePaused();
        //    //  Debug.Log("q3");
        //    ////searches through pauseObjects for PauseText
        //    //foreach (GameObject g in pauseObjects)
        //    //{
        //    //   // if (g.name == "PauseText")
        //    //        //makes PauseText to Inactive
        //    //        g.SetActive(false);

        //    //}
        //}
        if ((Input.GetKeyDown(KeyCode.P)))
        {

            if (HatGameController.instance.isPlaying)
            {
                Debug.Log("pressed");
                gameData.isGameLogging=false;
                gameData.isGameLogging = false;
                pauseControl();

            }
            else if (!HatGameController.instance.isPlaying)
            {


                HatGameController.instance.Restart();

            }
        }
    }


    public void Reload()
    {
        Application.LoadLevel(Application.loadedLevel);
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
    public void LoadLevel(string level)
    {
        Application.LoadLevel(level);
    }
}
