using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using NeuroRehabLibrary;

public class HatGameController : MonoBehaviour
{
    public static HatGameController instance;
    public bool isPlaying = false;
    public Text ScoreText;
    public Text timeLeftText;
    public GameObject GameOverObject;
    public GameObject StartButton;
    public Camera cam;
    public GameObject[] ball;
    Rigidbody2D rig2D;
    private float gameMoveTime = 0f;
    private float lastTimestamp = 0f;
    private float playSize;
    private float gameSpeed=1f;
    private float successRate=1f;
    public int score = 0;
    float maxwidth;
    float trialTime = 90;
    float timeLeft;
    public bool balldestroyed = true;
    int count;
    public bool isPressed = false;
    private GameSession currentGameSession;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);

        playSize = Camera.main.orthographicSize * Camera.main.aspect;


    }
    void Start()
    {
        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
        AppLogger.LogInfo($"{SceneManager.GetActiveScene().name} scene started.");

        foreach (GameObject b in ball)
        {
            rig2D = b.GetComponent<Rigidbody2D>();  
        }
        rig2D = this.gameObject.GetComponent<Rigidbody2D>();

        isPlaying = false;
        gameData.isGameLogging = true;
        timeLeftText = GameObject.FindGameObjectWithTag("TimeLeftText").GetComponent<Text>();
        ScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<Text>();
        StartButton.SetActive(false);
        if (cam == null)
        {
            cam = Camera.main;
        }
        lastTimestamp = Time.unscaledTime;
        Vector3 UpperCorner = new Vector3(Screen.width, Screen.height, 0);
        float hatwidth = GameObject.Find("HatFrontSprite").GetComponent<Renderer>().bounds.extents.x;
        Vector3 targetWidth = cam.ScreenToWorldPoint(UpperCorner);
        maxwidth = targetWidth.x - hatwidth;
        PlutoComm.OnButtonReleased += onPlutoButtonReleased;
        UpdateText();
        HT_spawnTargets1.instance.playSize = maxwidth * 0.8f;
       // StartNewGameSession();
    }
    void Update()
    {
        if (Time.timeScale > 0 && isPlaying)
        {
            float currentTime = Time.unscaledTime;
            gameMoveTime += currentTime - lastTimestamp;
            //Debug.Log("ang " +PlutoComm.angle);
            lastTimestamp = currentTime;
        }
        else
        {
            lastTimestamp = Time.unscaledTime; // Update timestamp even if paused or finished
        }

        if (isPressed && !isPlaying)
        {
            Restart();
            isPressed= false;
        }
        if (isPlaying)
        {
            timeLeft -= Time.deltaTime;


            if (timeLeft < 0)
            {
                int win;
                timeLeft = 0;
               
                EndCurrentGameSession();
                if (balldestroyed)
                {
                    isPlaying = false;
                    gameData.isGameLogging=false;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    StartButton.SetActive(true);

                    if (HatGameController.instance.score > 0.8 * HT_spawnTargets1.instance.count)
                    {
                        win = 1;
                    }
                    else
                    {
                        win = -1;
                    }
                    score = 0;
                    HT_spawnTargets1.instance.count = 0;
                }
            }
        }
        UpdateText();
         gameData.moveTime = gameMoveTime;
    }

    public void SpawnTarget()
    {
        count++;
        GameObject target;

        if (timeLeft > 0 && balldestroyed)
        {
            balldestroyed = false;
            //HTDifficultyManager.ballSpeed = 2f + 0.3f * 1;
            HTDifficultyManager.ballSpeed = 2f + 0.3f *(1 +gameData.gameSpeedHT);
            HT_spawnTargets1.instance.trailDuration = (8.0f / HTDifficultyManager.ballSpeed) * 0.8f;

           float x = UnityEngine.Random.Range(-playSize +0.5f, playSize - 0.5f);
            Vector3 spawnPosition = new Vector3(
                x,
                6f,
                0
                );

            Quaternion spawnRotation = Quaternion.identity;

            int rand = UnityEngine.Random.Range(0, 5);
            if (rand < 5)
            {
                int i = UnityEngine.Random.Range(0, 2);
                target = Instantiate(ball[i], spawnPosition, spawnRotation);
                target.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -HTDifficultyManager.ballSpeed);
                target.transform.localScale = HTDifficultyManager.Scale;
            }
            else
            {
                Debug.Log("here");
                target = Instantiate(ball[0], spawnPosition, spawnRotation);
                target.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -HTDifficultyManager.ballSpeed);
            }

            HT_spawnTargets1.instance.stopClock = HT_spawnTargets1.instance.trailDuration;

        }
        else if( timeLeft<= 0)
        {
            EndCurrentGameSession();
        }
    }
    public void UpdateText()
    {
        timeLeftText.text = "Time left : " + ((int)timeLeft).ToString();
        ScoreText.text = "Score:"+ gameData.gameScore;
        if (gameData.gameScore>0 && gameData.gameScore<11 )
        {
            gameData.successRate = (float)gameData.gameScore / 10;
        }
    }
    private void onPlutoButtonReleased()
    {
        isPressed = true;
    }
    public void Restart()
    {
        balldestroyed = true;
        isPlaying = true;
         gameMoveTime = 0f;
        lastTimestamp = Time.unscaledTime;
        StartNewGameSession();
        gameData.isGameLogging = true;
        timeLeft = trialTime;
        AppLogger.LogInfo("HatGame Started");
        SpawnTarget();


    }
    void StartNewGameSession()
    {
        currentGameSession = new GameSession
        {
            GameName = "HAT-Trick",
            Assessment = 0
        };

        SessionManager.Instance.StartGameSession(currentGameSession);
        Debug.Log($"Started new game session with session number: {currentGameSession.SessionNumber}");

        SetSessionDetails();
    }
    private void SetSessionDetails()
    {
        string device = "PLUTO";
        string assistMode = "Null";
        string assistModeParameters = "Null";
        string deviceSetupLocation = "CMC-Bioeng-dpt";
        string gameParameter = "YourGameParameter";
        string mech = AppData.selectedMechanism;
        SessionManager.Instance.SetDevice(device, currentGameSession);
        SessionManager.Instance.SetAssistMode(assistMode, assistModeParameters, currentGameSession);
        SessionManager.Instance.SetDeviceSetupLocation(deviceSetupLocation, currentGameSession);
        SessionManager.Instance.SetGameParameter(gameParameter, currentGameSession);
        SessionManager.Instance.mechanism(mech, currentGameSession);
    }
    void EndCurrentGameSession()
    {
        if (currentGameSession != null)
        {
            string trialdata = AppData.trialDataFileLocation;
            string movetime = gameData.moveTime.ToString("F0");
            SessionManager.Instance.SetTrialDataFileLocation(trialdata, currentGameSession);
            SessionManager.Instance.moveTime(movetime, currentGameSession);
            SessionManager.Instance.gameSpeed(gameData.gameSpeedHT,currentGameSession);
            SessionManager.Instance.successRate(gameData.successRate, currentGameSession);
            SessionManager.Instance.EndGameSession(currentGameSession);
        }
    }

    public void Reload()
    {
        //  SceneManager.LoadScene(SceneManager.L)
        //  GameOverObject.SetActive(false);
    }
    public void exitGame()
    {
        EndCurrentGameSession();
        SceneManager.LoadScene("choosegame");
    }
}

