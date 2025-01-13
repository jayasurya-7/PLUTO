using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using NeuroRehabLibrary;
using TMPro;

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
    private float x;
    private GameSession currentGameSession;

    //AAN


    // Control variables
    private bool isRunning = false;
    //private float controlTarget = 0.0f;
    //private float controlBound = 0.0f;
    private const float tgtDuration = 3.0f;
    private float _currentTime = 0;
    private float _initialTarget = 0;
    private float _finalTarget = 0;
    //private bool _changingTarget = false;

    // Discrete movements related variables
    private uint trialNo = 0;
    // Define variables for a discrete movement state machine
    // Enumerated variable for states
    private enum DiscreteMovementTrialState
    {
        Rest,           // Resting state
        SetTarget,      // Set the target
        Moving,         // Start Movement.
        Success,        // Successfull reach
        Failure,        // Failed reach
    }
    private DiscreteMovementTrialState _trialState;
    private static readonly IReadOnlyList<float> stateDurations = Array.AsReadOnly(new float[] {
        1.00f,          // Rest duration
        0.25f,          // Target set duration
        5.00f,          // Maximum movement duration
        0.25f,          // Successful reach
        0.25f,          // Failed reach
    });
    private const float tgtHoldDuration = 1f;
    private float _trialTarget = 0f;
    private float _currTgtForDisplay;
    private float trialDuration = 0f;
    private float stateStartTime = 0f;
    private float _tempIntraStateTimer = 0f;

    // Control bound adaptation variables
    private float prevControlBound = 0.16f;
    // Magical minimum value where the mechanisms mostly move without too much instability.
    private float currControlBound = 0.16f;
    private const float cbChangeDuration = 2.0f;
    private sbyte currControlDir = 0;
    private float _currCBforDisplay;
    //private int successRate;
   // public Button btnStartStop;


    // AAN class
    private PlutoAANController aanCtrler;

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

        // Set Control mode.
        
        //successRate = 0;
        // Start the state machine.

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

        PlutoComm.setControlType("POSITIONAAN");
        // StartNewGameSession();
    }
    void Update()
    {
        PlutoComm.sendHeartbeat();
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
        if (PlutoComm.CONTROLTYPE[PlutoComm.controlType]!= "POSITIONAAN")
        {
            PlutoComm.setControlType("POSITIONAAN");
        }

       // RunTrialStateMachine();

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
        if (isRunning == false) return;

        // Update trial time
        trialDuration += Time.deltaTime;

        // Run trial state machine
        RunTrialStateMachine();
    }

    private void RunTrialStateMachine()
    {
        float _deltime = trialDuration - stateStartTime;
        bool _statetimeout = _deltime >= stateDurations[(int)_trialState];
        // Time when target is reached.
       
        bool _tgtreached = Math.Abs(_trialTarget - PlutoComm.angle) <= 5.0f;
        switch (_trialState)
        {
            case DiscreteMovementTrialState.Rest:
                // Check if the rest time has run out.
                if (_statetimeout)
                {
                    SetTrialState(DiscreteMovementTrialState.SetTarget);
                    Debug.Log("JS"); 
                }
                break;
            case DiscreteMovementTrialState.SetTarget:
                if (_statetimeout)
                {
                    SetTrialState(DiscreteMovementTrialState.Moving);
                    Debug.Log("JSx");
                }
                break;
            case DiscreteMovementTrialState.Moving:
                // Update control bound smoothly.
                UpdateControlBoundSmoothly();
                // Update the position control target smoothly.
                UpdatePositionTargetSmoothly();

                // Check if the target has been reached
                if (_tgtreached)
                {
                    _tempIntraStateTimer += Time.deltaTime;
                    isRunning = false;
                }
                else
                {
                    _tempIntraStateTimer = 0;
                }
                // Check if target time has been reached.
                if (_tempIntraStateTimer >= tgtHoldDuration || Math.Abs(PlutoComm.angle) == _finalTarget)
                {
                    SetTrialState(DiscreteMovementTrialState.Success);
                }
                else if (_statetimeout)
                {
                    SetTrialState(DiscreteMovementTrialState.Failure);
                }
                break;
            case DiscreteMovementTrialState.Success:
            case DiscreteMovementTrialState.Failure:
                if (_statetimeout) SetTrialState(DiscreteMovementTrialState.Rest);
                break;
        }
    }

    private void SetTrialState(DiscreteMovementTrialState newState)
    {
        _trialState = newState;
        switch (newState)
        {
            case DiscreteMovementTrialState.Rest:
                trialDuration = 0f;
                prevControlBound = PlutoComm.controlBound;
                currControlBound = aanCtrler.getControlBoundForTrial();
                trialNo += 1;
                Debug.Log("Checking");
                // Reset target timer (for display purposes).
                _tempIntraStateTimer = 0f;
                break;
            case DiscreteMovementTrialState.SetTarget:
                // Random select target from the appropriate range.
                float _tgtscale = UnityEngine.Random.Range(0.0f, 1.0f);
                _trialTarget = x;
                break;
            case DiscreteMovementTrialState.Moving:
                // Start the position control to the tatget location.
                _initialTarget = PlutoComm.angle;
                _finalTarget = x;
                // Set new trial target.
                aanCtrler.setNewTrialDetails(_initialTarget, _finalTarget);
                // Set control direction
                PlutoComm.setControlDir(aanCtrler.getControlDirectionForTrial());
                Debug.Log("Value of CB:" + aanCtrler.getControlDirectionForTrial());
                _tempIntraStateTimer = 0f;
                break;
            case DiscreteMovementTrialState.Success:
                // Update trial result.
                Debug.Log("Success");
                isRunning = false;
                SetTrialState(DiscreteMovementTrialState.Rest);
                //PlutoComm.setControlType("NONE");

                //aanCtrler.upateTrialResult(true);
                // Update adaptation row.
                // WriteTrialRowInfo(1);
                break;
            case DiscreteMovementTrialState.Failure:
                //aanCtrler.upateTrialResult(false);
                Debug.Log("Failure");
                isRunning = false;
                SetTrialState(DiscreteMovementTrialState.Rest);
                // PlutoComm.setControlType("NONE");
                //WriteTrialRowInfo(0);
                break;
        }
        stateStartTime = trialDuration;
    }



    private void UpdateControlBoundSmoothly()
    {
        PlutoComm.setControlBound(0.74f);
        if ((prevControlBound == currControlBound) ||
            ((trialDuration - stateStartTime) >= cbChangeDuration))
        {
            return;
        }
    }

    private void UpdatePositionTargetSmoothly()
    {
        float _t = (trialDuration - stateStartTime) / tgtDuration;
        // Limit _t between 0 and 1.
        _t = Mathf.Clamp(_t, 0, 1);
        // Compute the current target value using the minimum jerk trajectory.
        _currTgtForDisplay = _initialTarget + (_finalTarget - _initialTarget) * (10 * Mathf.Pow(_t, 3) - 15 * Mathf.Pow(_t, 4) + 6 * Mathf.Pow(_t, 5));
        // Update position target
        // 
        PlutoComm.setControlTarget(_currTgtForDisplay);
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

            x = UnityEngine.Random.Range(-playSize +0.5f, playSize - 0.5f);
            Debug.Log("position :"+ x);
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
        aanCtrler = new PlutoAANController();
        // Change button text
        isRunning = true;
        SetTrialState(DiscreteMovementTrialState.Rest);
        PlutoComm.setControlType("POSITIONAAN");
        PlutoComm.setControlBound(currControlBound);
        PlutoComm.setControlDir(0);
        trialNo = 0;
        SetTrialState(DiscreteMovementTrialState.Moving );
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

