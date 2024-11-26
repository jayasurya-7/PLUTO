
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class FlappyGameControl : MonoBehaviour
{
    public AudioClip[] winClip;
    public AudioClip[] hitClip;
    public Text ScoreText;
    //public ProgressBar timerObject;
    public static FlappyGameControl instance;
    //public RockVR.Video.VideoCapture vdc;
    public GameObject GameOverText;
    public bool gameOver = false;
    public float scrollSpeed = -3f;
    private int score;
    public GameObject[] pauseObjects;
    public float gameduration = 90;
    public GameObject start;
    int win = 0;
    bool endValSet = false;


    public BirdControl bc;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        AppData.initializeStuff();
        Time.timeScale = 1;
       
        

    }

    // Update is called once per frame
    void Update()
    {
        UpdateGameDurationUI();

        //uses the p button to pause and unpause the game
        if (Input.GetKeyDown(KeyCode.P) )
        {
            if (!gameOver)
            {
                if (Time.timeScale == 1)
                {
                    Time.timeScale = 0;
                    showPaused();
                }
                else if (Time.timeScale == 0)
                {
                    Time.timeScale = 1;
                    hidePaused();
                }
            }
            else if (gameOver)
            {
                hidePaused();
                playAgain();
            }
        }




    }


    void UpdateGameDurationUI()
    {
       // timerObject.specifiedValue = Mathf.Clamp(100 * (90 - gameduration) / 90f, 0, 100); ;

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
    public void BirdDied()
    {
        endValSet = true;
        //if (win == 1)
        //{
        //    GameOverText.GetComponent<Text>().text = "Great Work! \n You WON! :)";
        //    switch (AppData.startGamePerformace)
        //    {

        //        case 2:
        //            GameOverText.GetComponent<Text>().text = "Great Work! Level Increased! :) \n"
        //                + "Current Level :" + AppData.endGameLevelSpeed;

        //            break;

        //    }
        //}
        if (win == -1)
            GameOverText.GetComponent<Text>().text = "Try Again";
        GameOverText.SetActive(true);
        gameOver = true;

    }
    public void BirdScored()
    {


        if (gameduration < 0 && !endValSet)
        {
            //FB_spawnTargets.instance.setZero();
            gameduration = 0;


            if (!gameOver)
            {
                win = 1;
            }
            else
            {
                win = -1;
            }
            gameOver = true;
            Debug.Log(win);
            score = 0;
            BirdDied();

        }
        else
        {
            if (!bc.startBlinking)
            {
                int index = UnityEngine.Random.Range(0, winClip.Length);
                GetComponent<AudioSource>().clip = winClip[index];
                if (score != 0)
                {
                    GetComponent<AudioSource>().Play();
                }
                score += 1;
            }
            else
            {
                int index = UnityEngine.Random.Range(0, hitClip.Length);
                GetComponent<AudioSource>().clip = hitClip[index];
                GetComponent<AudioSource>().Play();

            }
            //FB_spawnTargets.instance.reached = true;

            ScoreText.text = "Score: " + score.ToString();
            FlappyColumnPool.instance.spawnColumn();
        }
    }
    public void playAgain()
    {
        if (gameOver == true)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        }
        if (!gameOver)
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
                showPaused();
            }
            else if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
                hidePaused();
            }

        }

    }
    public void PlayStart()
    {
        endValSet = false;
        start.SetActive(false);
        Time.timeScale = 1;
    }

    public void continueButton()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            hidePaused();

        }
    }


}
