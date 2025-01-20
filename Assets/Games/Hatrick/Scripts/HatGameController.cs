//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using NeuroRehabLibrary;
//using TMPro;
//using System;

//public class HatGameController : MonoBehaviour
//{
//    public static HatGameController instance;

//    public Text ScoreText;
//    public Text timeLeftText;
//    public GameObject GameOverObject;
//    public GameObject StartButton;
//    public GameObject PauseButton;
//    public GameObject ResumeButton;
//    public Camera cam;
//    public GameObject[] ball;

//    private Rigidbody2D rig2D;
//    private float gameMoveTime = 0f;
//    private float lastTimestamp = 0f;
//    private float playSize;
//    private float gameSpeed = 1f;
//    private float successRate = 1f;
//    public int score = 0;
//    private float maxwidth;
//    private float trialTime = 90f;
//    private float timeLeft;
//    public bool balldestroyed = true;
//    private bool isPressed = false;
//    private bool isPaused = false;
//    private int count;
//    private float x;

//    private GameSession currentGameSession;

//    private enum GameState { NotStarted, Playing, Paused, GameOver }
//    private GameState currentState = GameState.NotStarted;
//    private bool isPlaying = false; // Tracks whether the game is currently active
//    private float Player;


//    //AAN


//    // Control variables
//    private bool isRunning = false;
//    //private float controlTarget = 0.0f;
//    //private float controlBound = 0.0f;
//    private const float tgtDuration = 3.0f;
//    private float _currentTime = 0;
//    private float _initialTarget = 0;
//    private float _finalTarget = 0;
//    //private bool _changingTarget = false;

//    // Discrete movements related variables
//    private uint trialNo = 0;
//    // Define variables for a discrete movement state machine
//    // Enumerated variable for states
//    private enum DiscreteMovementTrialState
//    {
//        Rest,           // Resting state
//        SetTarget,      // Set the target
//        Moving,         // Start Movement.
//        Success,        // Successfull reach
//        Failure,        // Failed reach
//    }
//    private DiscreteMovementTrialState _trialState;
//    private static readonly IReadOnlyList<float> stateDurations = Array.AsReadOnly(new float[] {
//        1.00f,          // Rest duration
//        0.25f,          // Target set duration
//        3.00f,          // Maximum movement duration
//        0.25f,          // Successful reach
//        0.25f,          // Failed reach
//    });
//    private const float tgtHoldDuration = 1f;
//    private float _trialTarget = 0f;
//    private float _currTgtForDisplay;
//    private float trialDuration = 0f;
//    private float stateStartTime = 0f;
//    private float _tempIntraStateTimer = 0f;

//    // Control bound adaptation variables
//    private float prevControlBound = 0.16f;
//    // Magical minimum value where the mechanisms mostly move without too much instability.
//    private float currControlBound = 0.16f;
//    private const float cbChangeDuration = 2.0f;
//    private sbyte currControlDir = 0;
//    private float _currCBforDisplay;
//    //private int successRate;
//    // public Button btnStartStop;


//    // AAN class
//    private PlutoAANController aanCtrler;
//    public bool IsPlaying // Expose the variable as read-only if needed elsewhere
//    {
//        get { return isPlaying; }
//    }

//    private void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }

//        playSize = Camera.main.orthographicSize * Camera.main.aspect;
//    }

//    void Start()
//    {
//        InitializeGame();
//    }

//    void Update()
//    {
//        PlutoComm.sendHeartbeat();
//        PlutoComm.setControlType("POSITIONAAN");
//        if (currentState == GameState.Playing)
//        {
//            HandleGameUpdate();
//        }
//        if (isPressed)
//        {
//            if (!isPlaying && !isPaused)
//            {
//                StartGame();
//                isPressed = false;
//            }else if (isPlaying && !isPaused)
//            {
//                PauseGame();
//                isPressed = false;
//            }
//            else if (isPlaying && isPaused)
//            {
//                ResumeGame();
//                isPressed = false;
//            }
//        }

//         Player = GameObject.FindGameObjectWithTag("Player").transform.position.x;
//        // Update trial time
//        trialDuration += Time.deltaTime;
//        if (isRunning) { 
//            RunTrialStateMachine();
//        }
//        // Run trial state machine
//        Debug.Log("_trialState                  ;" + _trialState);
//    }

//    public void StartGame()
//    {
//        if (currentState == GameState.NotStarted || currentState == GameState.Paused)
//        {
//            currentState = GameState.Playing;
//            isPlaying = true;
//            timeLeft = trialTime;
//            lastTimestamp = Time.unscaledTime;
//            gameMoveTime = 0f;
//            aanCtrler = new PlutoAANController();
//            // Change button text
//            isRunning = true;
//            SetTrialState(DiscreteMovementTrialState.Rest);
//            PlutoComm.setControlType("POSITIONAAN");
//            PlutoComm.setControlBound(currControlBound);
//            PlutoComm.setControlDir(0);
//            trialNo = 0;

//            StartNewGameSession();
//            gameData.isGameLogging = true;

//            StartButton.SetActive(false);
//            PauseButton.SetActive(true);
//            ResumeButton.SetActive(false);

//            AppLogger.LogInfo("Game Started.");
//            SpawnTarget();
//        }
//    }

//    public void PauseGame()
//    {
//        if (currentState == GameState.Playing)
//        {
//            currentState = GameState.Paused;
//            isPlaying = false;
//            isPaused = true;
//            Time.timeScale = 0;
//            PauseButton.SetActive(false);
//            ResumeButton.SetActive(true);

//            AppLogger.LogInfo("Game Paused.");
//        }
//    }

//    public void ResumeGame()
//    {
//        if (currentState == GameState.Paused)
//        {
//            currentState = GameState.Playing;
//            isPlaying = true;

//            Time.timeScale = 1;
//            PauseButton.SetActive(true);
//            ResumeButton.SetActive(false);

//            AppLogger.LogInfo("Game Resumed.");
//        }
//    }

//    public void RestartGame()
//    {
//        currentState = GameState.NotStarted;
//        isPlaying = false;
//        score = 0;
//        HT_spawnTargets1.instance.count = 0;

//        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//    }

//    private void HandleGameUpdate()
//    {
//        PlutoComm.sendHeartbeat();

//        if (Time.timeScale > 0 && isPlaying)
//        {
//            float currentTime = Time.unscaledTime;
//            gameMoveTime += currentTime - lastTimestamp;
//            lastTimestamp = currentTime;

//            timeLeft -= Time.deltaTime;
//            if (timeLeft <= 0)
//            {
//                timeLeft = 0;
//                GameOver();
//            }
//        }

//        UpdateText();
//        gameData.moveTime = gameMoveTime;
//    }

//    private void GameOver()
//    {
//        currentState = GameState.GameOver;
//        isPlaying = false;
//        gameData.isGameLogging = false;

//        EndCurrentGameSession();
//        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

//        AppLogger.LogInfo("Game Over.");
//    }

//    public void SpawnTarget()
//    {
//        RunTrialStateMachine();
//        if (timeLeft > 0 && balldestroyed)
//        {
//            balldestroyed = false;

//            float ballSpeed = 2f + 0.3f * (1 + gameData.gameSpeedHT);
//            float trailDuration = (8.0f / ballSpeed) * 0.8f;
//            HT_spawnTargets1.instance.trailDuration = trailDuration;

//            x = UnityEngine.Random.Range(-playSize + 0.5f, playSize - 0.5f);
//            Vector3 spawnPosition = new Vector3(x, 6f, 0);
//            Quaternion spawnRotation = Quaternion.identity;

//            GameObject target = Instantiate(
//                ball[UnityEngine.Random.Range(0, ball.Length)],
//                spawnPosition,
//                spawnRotation
//            );
//            target.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -ballSpeed);
//            target.transform.localScale = HTDifficultyManager.Scale;

//            HT_spawnTargets1.instance.stopClock = trailDuration;
//        }
//    }

//    private void InitializeGame()
//    {
//        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
//        AppLogger.LogInfo($"{SceneManager.GetActiveScene().name} scene initialized.");

//        rig2D = GetComponent<Rigidbody2D>();
//        gameData.isGameLogging = false;

//        timeLeftText = GameObject.FindGameObjectWithTag("TimeLeftText").GetComponent<Text>();
//        ScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<Text>();

//        StartButton.SetActive(true);
//        PauseButton.SetActive(false);
//        ResumeButton.SetActive(false);

//        if (cam == null)
//        {
//            cam = Camera.main;
//        }

//        lastTimestamp = Time.unscaledTime;
//        maxwidth = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)).x - 0.5f;
//       // maxwidth = targetWidth.x - hatwidth;
//        PlutoComm.OnButtonReleased += onPlutoButtonReleased;
//        //UpdateText();
//        //HT_spawnTargets1.instance.playSize = maxwidth * 0.8f;
//        //PlutoComm.OnButtonReleased += () => { if (currentState == GameState.Paused) ResumeGame(); };
//    }

//    private void UpdateText()
//    {
//        timeLeftText.text = $"Time Left: {(int)timeLeft}";
//        ScoreText.text = $"Score: {gameData.gameScore}";
//    }

//    private void StartNewGameSession()
//    {
//        currentGameSession = new GameSession
//        {
//            GameName = "HAT-Trick",
//            Assessment = 0
//        };

//        SessionManager.Instance.StartGameSession(currentGameSession);
//        AppLogger.LogInfo($"Game session {currentGameSession.SessionNumber} started.");
//    }

//    private void EndCurrentGameSession()
//    {
//        if (currentGameSession != null)
//        {
//            SessionManager.Instance.SetTrialDataFileLocation(AppData.trialDataFileLocation, currentGameSession);
//            SessionManager.Instance.moveTime(gameData.moveTime.ToString("F0"), currentGameSession);
//            SessionManager.Instance.gameSpeed(gameData.gameSpeedHT, currentGameSession);
//            SessionManager.Instance.successRate(gameData.successRate, currentGameSession);
//            SessionManager.Instance.EndGameSession(currentGameSession);
//        }
//    }
//    public void exitGame()
//    {
//        //EndCurrentGameSession();
//        SceneManager.LoadScene("choosegame");
//    }
//    private void onPlutoButtonReleased()
//    {
//        isPressed = true;
//    }

//    private void RunTrialStateMachine()
//    {
//        float _deltime = trialDuration - stateStartTime;
//        bool _statetimeout = _deltime >= stateDurations[(int)_trialState];
//        // Time when target is reached.

//        bool _tgtreached = Math.Abs(_trialTarget - PlutoComm.angle) <= 5.0f;
//        switch (_trialState)
//        {
//            case DiscreteMovementTrialState.Rest:
//                // Check if the rest time has run out.
//                if (_statetimeout)
//                {
//                    SetTrialState(DiscreteMovementTrialState.SetTarget);
//                    Debug.Log("JS");
//                }
//                break;
//            case DiscreteMovementTrialState.SetTarget:
//                if (_statetimeout)
//                {
//                    SetTrialState(DiscreteMovementTrialState.Moving);
//                    Debug.Log("JSx");
//                }
//                break;
//            case DiscreteMovementTrialState.Moving:
//                // Update control bound smoothly.
//                UpdateControlBoundSmoothly();
//                // Update the position control target smoothly.
//                UpdatePositionTargetSmoothly();

//                // Check if the target has been reached
//                if (_tgtreached)
//                {
//                    _tempIntraStateTimer += Time.deltaTime;
//                    isRunning = false;
//                    Debug.Log("Target Reached");
//                }
//                else
//                {
//                    _tempIntraStateTimer = 0;
//                }
//                // Check if target time has been reached.
//                if (_tempIntraStateTimer >= tgtHoldDuration || Math.Abs(PlutoComm.angle) == _finalTarget)
//                {
//                    SetTrialState(DiscreteMovementTrialState.Success);
//                }
//                else if (_statetimeout)
//                {
//                    SetTrialState(DiscreteMovementTrialState.Failure);
//                }
//                break;
//            case DiscreteMovementTrialState.Success:
//            case DiscreteMovementTrialState.Failure:
//                if (_statetimeout) SetTrialState(DiscreteMovementTrialState.Rest);
//                break;
//        }
//    }

//    private void SetTrialState(DiscreteMovementTrialState newState)
//    {
//        _trialState = newState;
//        switch (newState)
//        {
//            case DiscreteMovementTrialState.Rest:
//                trialDuration = 0f;
//                prevControlBound = PlutoComm.controlBound;
//                currControlBound = aanCtrler.getControlBoundForTrial();
//                trialNo += 1;
//                // Reset target timer (for display purposes).
//                _tempIntraStateTimer = 0f;
//                break;
//            case DiscreteMovementTrialState.SetTarget:
//                // Random select target from the appropriate range.
//                float _tgtscale = UnityEngine.Random.Range(0.0f, 1.0f);
//                _trialTarget = x;
//                break;
//            case DiscreteMovementTrialState.Moving:
//                // Start the position control to the tatget location.
//                // _initialTarget = PlutoComm.angle;
//                _initialTarget = Player;
//                //_finalTarget = CalculateAngleFromX(x);
//                _finalTarget = x;
//                Debug.Log("yyy:"+ _initialTarget + "+" + _finalTarget);
//                // Set new trial target.
//                aanCtrler.setNewTrialDetails(_initialTarget, _finalTarget);
//                // Set control direction
//                PlutoComm.setControlDir(aanCtrler.getControlDirectionForTrial());
//                Debug.Log("Value of CB:" + aanCtrler.getControlDirectionForTrial());
//                _tempIntraStateTimer = 0f;
//                break;
//            case DiscreteMovementTrialState.Success:
//                // Update trial result.
//                Debug.Log("Success");
//               // isRunning = false;
//                SetTrialState(DiscreteMovementTrialState.Rest);
//                //PlutoComm.setControlType("NONE");

//                //aanCtrler.upateTrialResult(true);
//                // Update adaptation row.
//                // WriteTrialRowInfo(1);
//                break;
//            case DiscreteMovementTrialState.Failure:
//                //aanCtrler.upateTrialResult(false);
//                Debug.Log("Failure");
//                //isRunning = false;
//                SetTrialState(DiscreteMovementTrialState.Rest);
//                // PlutoComm.setControlType("NONE");
//                //WriteTrialRowInfo(0);
//                break;
//        }
//        stateStartTime = trialDuration;
//    }



//    private void UpdateControlBoundSmoothly()
//    {
//        PlutoComm.setControlBound(0.74f);
//        if ((prevControlBound == currControlBound) ||
//            ((trialDuration - stateStartTime) >= cbChangeDuration))
//        {
//            return;
//        }
//    }

//    private void UpdatePositionTargetSmoothly()
//    {
//        float _t = (trialDuration - stateStartTime) / tgtDuration;
//        // Limit _t between 0 and 1.
//        _t = Mathf.Clamp(_t, 0, 1);
//        // Compute the current target value using the minimum jerk trajectory.
//        _currTgtForDisplay = _initialTarget + (_finalTarget - _initialTarget) * (10 * Mathf.Pow(_t, 3) - 15 * Mathf.Pow(_t, 4) + 6 * Mathf.Pow(_t, 5));
//        // Update position target
//        // 
//        PlutoComm.setControlTarget(_currTgtForDisplay);
//    }
//    private float CalculateAngleFromX(float x)
//    {
//        // Assuming y = 1 for a fixed vertical reference
//        float y = 1.0f;

//        // Calculate the angle in degrees
//        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

//        // Ensure the angle is normalized (0 to 360 degrees)
//        if (angle < 0) angle += 360;
//        Debug.Log(angle + "angg");

//        return angle;
//    }

//    // Example in SetTrialState or SpawnTarget
//    private void ExampleUsage()
//    {
//        // Convert the x-position to an angle
//        float angle = CalculateAngleFromX(x);

//        // Use the angle for your mechanism
//        PlutoComm.setControlTarget(angle);

//        Debug.Log($"Converted X Position: {x} to Angle: {angle} degrees");
//    }



//}

















































































using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NeuroRehabLibrary;
using TMPro;
using System;
using Unity.Mathematics;

public class HatGameController : MonoBehaviour
{
    public static HatGameController instance;

    public Text ScoreText;
    public Text timeLeftText;
    public GameObject GameOverObject;
    public GameObject StartButton;
    public GameObject PauseButton;
    public GameObject ResumeButton;
    public Camera cam;
    public GameObject[] ball;

    private Rigidbody2D rig2D;
    private float gameMoveTime = 0f;
    private float lastTimestamp = 0f;
    private float playSize;
    private float gameSpeed = 1f;
    private float successRate = 1f;
    public int score = 0;
    private float maxwidth;
    private float trialTime = 90f;
    private float timeLeft;
    public bool balldestroyed = true;
    private bool isPressed = false;
    private bool isPaused = false;
    private int count;
    private float x;
    private float targetAngle;

    private GameSession currentGameSession;

    private bool isPlaying = false; // Tracks whether the game is currently active
    private float Player;
    private sbyte direction;

    //AAN

    private enum GameState { NotStarted, Playing, Paused, GameOver }
    private GameState currentState = GameState.NotStarted;

    private enum DiscreteMovementTrialState { Rest,Moving }
    private DiscreteMovementTrialState trialState = DiscreteMovementTrialState.Rest;

    private float targetPosition;
    private float playerPosition;
    private const float positionThreshold = 0.1f; // Threshold to determine if the target is reached


    // AAN class
    private bool isRunning = false;
    //private float controlTarget = 0.0f;
    //private float controlBound = 0.0f;
    private const float tgtDuration = 3.0f;
    private float _currentTime = 0;
    private float _initialTarget = 0;
    private float _finalTarget = 0;
    public bool targetSpwan= false;

    // Discrete movements related variables
    private uint trialNo = 0;
    // Define variables for a discrete movement state machine
    // Enumerated variable for states
    private float y;
    private DiscreteMovementTrialState _trialState;
    private static readonly IReadOnlyList<float> stateDurations = Array.AsReadOnly(new float[] {
        0.50f,          // Rest duration
        0.20f,
        4.00f,          // Maximum movement duration
    });
    private const float tgtHoldDuration = 1f;
    private float _trialTarget = 0f;
    private float _currTgtForDisplay;
    private float trialDuration = 0f;
    private float stateStartTime = 0f;
    private float _tempIntraStateTimer = 0f;
    public bool aromRangeSpawn=false;
    public Toggle spawnAreaToggle; // Add this to the Inspector
    // Control bound adaptation variables
    private float prevControlBound = 0.3f;
    // Magical minimum value where the mechanisms mostly move without too much instability.
    private float currControlBound = 0.3f;
    private const float cbChangeDuration = 2.0f;
    private sbyte currControlDir = 0;
    private float _currCBforDisplay;
    //private int successRate;
    public Image targetImage; // Assign this in the Inspector
    private int randomTargetIndex;
    private int spawnCounter = 0;
    private System.Random random = new System.Random();
    private PlutoAANController aanCtrler;
    public bool IsPlaying // Expose the variable as read-only if needed elsewhere
    {
        get { return isPlaying; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        if (spawnAreaToggle != null)
        {
            spawnAreaToggle.onValueChanged.AddListener(OnToggleSpawnArea);
        }
        playSize = Camera.main.orthographicSize * Camera.main.aspect;
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        PlutoComm.sendHeartbeat();
        if (PlutoComm.CONTROLTYPE[PlutoComm.controlType] == "NONE") {
            PlutoComm.setControlType("POSITIONAAN");
        }
        if (currentState == GameState.Playing)
        {
            HandleGameUpdate();
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position.x;
            //RunTrialStateMachine();
        }
        if (isPressed)
        {
            if (!isPlaying && !isPaused)
            {
                StartGame();
                isPressed = false;
            }
            else if (isPlaying && !isPaused)
            {
                PauseGame();
                isPressed = false;
            }
            else if (isPlaying && isPaused)
            {
                ResumeGame();
                isPressed = false;
            }
        }
        RunTrialStateMachine();

        // Update trialDuration (assuming it's in seconds)
        if (_trialState == DiscreteMovementTrialState.Moving)
        {
            trialDuration += Time.deltaTime;
        }
        Player = GameObject.FindGameObjectWithTag("Player").transform.position.x;
        //Debug.Log("targetSpawn        ;" + targetSpwan);
    }


  private void RunTrialStateMachine()
{
    // Use a timer to track state durations
    trialDuration += Time.deltaTime;

    switch (_trialState)
    {
        case DiscreteMovementTrialState.Rest:
            // Check if a target has spawned and wait 0.5 seconds before transitioning to Moving state
            if (targetSpwan && trialDuration >= 0.25f)
            {
                SetTrialState(DiscreteMovementTrialState.Moving);
            }
            break;

        case DiscreteMovementTrialState.Moving:
                // Update smooth transitions for control bound and target
                if (targetSpwan)
                {
                    UpdateControlBoundSmoothly();
                    UpdatePositionTargetSmoothly();

                    // Transition back to Rest after 4 seconds
                    if (trialDuration >= 4.5f)
                    {
                        Debug.Log("Target reached. Returning to Rest state.");
                        SetTrialState(DiscreteMovementTrialState.Rest);
                    }
                }
                else
                {
                    Debug.Log("Not executed");
                }
            
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
            targetSpwan = false; // Reset target spawn flag
            break;

        case DiscreteMovementTrialState.Moving:
            trialDuration = 0f;
            // Set initial and final targets for smooth transition
            _initialTarget = PlutoComm.angle;
            _finalTarget = targetAngle;

            // Update control direction based on target position
            PlutoComm.setControlDir((sbyte)(targetPosition > playerPosition ? 1 : -1));

            // Initialize new trial details
            aanCtrler.setNewTrialDetails(_initialTarget, _finalTarget);
            break;
    }
}

    private float SpawnTargetArea()
    {
        // Get the AROM range
        AppData.newAROM = new AROM(AppData.selectedMechanism);
        float aromMin = AppData.newAROM.tmin; // Minimum angle for AROM
        float aromMax = AppData.newAROM.tmax; // Maximum angle for AROM

        // Map the AROM range to the smaller playSize within the PROM playSize
        float xMin = MapAROMToPROMPlaySize(aromMin);
        float xMax = MapAROMToPROMPlaySize(aromMax);

        // Generate a random target position within the smaller mapped range
        float targetPosition = UnityEngine.Random.Range(xMin, xMax);

        Debug.Log($"Spawned Target Area Position: {targetPosition} (AROM Min: {aromMin}, Max: {aromMax}, Mapped X Min: {xMin}, Mapped X Max: {xMax})");
        return targetPosition;
    }

    // Helper function to map an AROM angle to a smaller range within the PROM playSize
    private float MapAROMToPROMPlaySize(float angle)
    {
        // Get the PROM range
        AppData.newPROM = new MechanismData(AppData.selectedMechanism);
        float promMin = AppData.newPROM.tmin; // Minimum angle for PROM
        float promMax = AppData.newPROM.tmax; // Maximum angle for PROM

        // Calculate the PROM playSize range
        float promRange = promMax - promMin;

        // Calculate the normalized AROM range within PROM (e.g., AROM is 50% of PROM range)
        float normalizedAROM = (angle - promMin) / promRange;

        // Scale to a smaller playSize range (e.g., 50% of PROM playSize)
        float scalingFactor = 0.5f; // Adjust this as needed to set how much smaller AROM is
        float adjustedRange = scalingFactor * 2 * playSize;

        return Mathf.Lerp(-adjustedRange / 2, adjustedRange / 2, normalizedAROM);
    }

    public void StartGame()
    {
        if (currentState == GameState.NotStarted || currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            isPlaying = true;
            timeLeft = trialTime;
            lastTimestamp = Time.unscaledTime;
            gameMoveTime = 0f;
            aanCtrler = new PlutoAANController();
            // Change button text
            trialState = DiscreteMovementTrialState.Moving;

            StartNewGameSession();
            gameData.isGameLogging = true;

            StartButton.SetActive(false);
            PauseButton.SetActive(true);
            ResumeButton.SetActive(false);

            AppLogger.LogInfo("Game Started.");
            SpawnTarget();
        }
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            isPlaying = false;
            isPaused = true;
            Time.timeScale = 0;
            PauseButton.SetActive(false);
            ResumeButton.SetActive(true);

            AppLogger.LogInfo("Game Paused.");
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            isPlaying = true;

            Time.timeScale = 1;
            PauseButton.SetActive(true);
            ResumeButton.SetActive(false);

            AppLogger.LogInfo("Game Resumed.");
        }
    }

    public void RestartGame()
    {
        currentState = GameState.NotStarted;
        isPlaying = false;
        score = 0;
        HT_spawnTargets1.instance.count = 0;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HandleGameUpdate()
    {
        PlutoComm.sendHeartbeat();

        if (Time.timeScale > 0 && isPlaying)
        {
            float currentTime = Time.unscaledTime;
            gameMoveTime += currentTime - lastTimestamp;
            lastTimestamp = currentTime;

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                timeLeft = 0;
                GameOver();
            }
        }

        UpdateText();
        gameData.moveTime = gameMoveTime;
    }

    private void GameOver()
    {
        currentState = GameState.GameOver;
        isPlaying = false;
        gameData.isGameLogging = false;

        EndCurrentGameSession();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        AppLogger.LogInfo("Game Over.");
    }

    // Toggle callback to enable or disable spawn area
    private void OnToggleSpawnArea(bool isEnabled)
    {
        aromRangeSpawn = isEnabled;
        Debug.Log("Spawn Area Enabled: " + isEnabled);
    }
    public void SpawnTarget()
    {
        if (timeLeft > 0 && balldestroyed)
        {
            balldestroyed = false;
            float ballSpeed = 2f + 0.3f * (1 + gameData.gameSpeedHT);
            float trailDuration = (8.0f / ballSpeed) * 0.8f;
            HT_spawnTargets1.instance.trailDuration = trailDuration;
            Debug.Log("SPAWNNING");
            // Use SpawnTargetArea if toggle is enabled
            if (aromRangeSpawn)
            {
                targetPosition = SpawnTargetArea();
            }
            else
            {
                x = UnityEngine.Random.Range(-playSize + 0.5f, playSize - 0.5f);
                //y = UnityEngine.Random.Range(-170.0f, 90.0f);
                targetPosition = x;

            }
            Vector3 spawnPosition = new Vector3(targetPosition, 6f, 0);
            Quaternion spawnRotation = Quaternion.identity;

            // Randomly select a ball to instantiate
            int ballIndex = UnityEngine.Random.Range(0, ball.Length);
            GameObject target = Instantiate(
                ball[ballIndex],
                spawnPosition,
                spawnRotation
            );
            targetSpwan = ((ballIndex == 0)|| (ballIndex == 2)|| (ballIndex == 3));

            target.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -ballSpeed);
            target.transform.localScale = HTDifficultyManager.Scale;

            HT_spawnTargets1.instance.stopClock = trailDuration;

            //targetPosition = x; // Set target position for the mechanism
            targetAngle = ScreenPositionToAngle(targetPosition); // Convert to angle
           // Debug.Log("ta :"+ targetAngle);
            if (targetSpwan)
            {

                //float targetAngle = (PlutoComm.angle > 0f) ? -60f : 60f;
               // sbyte dir = (sbyte)(targetPosition > playerPosition ? 1 : -1);
                // Debug.Log("valuee 1 :" + (targetPosition > playerPosition ? 1 : -1));
                //Debug.Log("valuee 2 :" + targetPosition + "  +  "+ PlutoComm.CONTROLTYPE[PlutoComm.controlType]);
               // PlutoComm.setControlDir(dir);
               // PlutoComm.setControlBound(0.25f);
                //PlutoComm.setControlTarget(targetAngle);
            }
            // Check if the current spawn matches the randomly chosen index
            if (spawnCounter == randomTargetIndex)
            {
                targetImage.gameObject.SetActive(true); // Display the image
                Debug.Log("Displaying the target image!");
            }
            else
            {
                targetImage.gameObject.SetActive(false); // Hide the image
            }

            spawnCounter++; // Increment spawn counter
        }
    }
    // private void UpdateControlBoundSmoothly()
    // {
    //     // Only update control bound if the target has been spawned.
    //     if (!targetSpwan) return;

    //     // Time variable for smooth transition of control bounds.
    //     float _t = (trialDuration - stateStartTime) / cbChangeDuration;
    //     _t = Mathf.Clamp(_t, 0, 1); // Clamp t between 0 and 1.

    //     // Compute the control bound value using the minimum jerk trajectory.
    //     _currCBforDisplay = prevControlBound + (currControlBound - prevControlBound) * (10 * Mathf.Pow(_t, 3) - 15 * Mathf.Pow(_t, 4) + 6 * Mathf.Pow(_t, 5));

    //     // Update the control bound in the system.
    //     PlutoComm.setControlBound(_currCBforDisplay);
    // }

    // private void UpdatePositionTargetSmoothly()
    // {
    //     // Only update position target if the target has been spawned.
    //     if (!targetSpwan) return;

    //     // Time variable for smooth transition of target position.
    //     float _t = (trialDuration - stateStartTime) / tgtDuration;
    //     _t = Mathf.Clamp(_t, 0, 1); // Clamp t between 0 and 1.

    //     // Compute the current target value using the minimum jerk trajectory.
    //     _currTgtForDisplay = _initialTarget + (_finalTarget - _initialTarget) * (10 * Mathf.Pow(_t, 3) - 15 * Mathf.Pow(_t, 4) + 6 * Mathf.Pow(_t, 5));

    //     // Update the position target in the system.
    //     PlutoComm.setControlTarget(_currTgtForDisplay);
    // }

    private void UpdateControlBoundSmoothly()
{
    if (!targetSpwan) return;
    // Interpolation factor (t) for smooth transition
    float t = trialDuration / 4.5f; // Normalize by the Moving state duration (4 seconds)

    // Smoothly interpolate control bound from its current position to the target
    float smoothedControlBound = Mathf.Lerp(0.1f, 0.6f, t);

    // Update the control direction
    PlutoComm.setControlBound(smoothedControlBound);

    // Optionally log or debug for visualization
    //Debug.Log($"Control Bound updated smoothly to: {smoothedControlBound}");
}
private void UpdatePositionTargetSmoothly()
{
    // Interpolation factor (t) for smooth transition
    float t = trialDuration / 4f; // Normalize by the Moving state duration (4 seconds)

    // Smoothly interpolate the target position
    float smoothedTargetPosition = Mathf.Lerp(_initialTarget, _finalTarget, t);

    // Update the target position
    PlutoComm.setControlTarget(smoothedTargetPosition);

    // Optionally log or debug for visualization
    //Debug.Log($"Target position updated smoothly to: {smoothedTargetPosition}");
}


    private void InitializeGame()
    {
        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
        AppLogger.LogInfo($"{SceneManager.GetActiveScene().name} scene initialized.");

        rig2D = GetComponent<Rigidbody2D>();
        gameData.isGameLogging = false;

        timeLeftText = GameObject.FindGameObjectWithTag("TimeLeftText").GetComponent<Text>();
        ScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<Text>();

        StartButton.SetActive(true);
        PauseButton.SetActive(false);
        ResumeButton.SetActive(false);

        if (cam == null)
        {
            cam = Camera.main;
        }
        PlutoComm.setControlType("POSITIONAAN");
        lastTimestamp = Time.unscaledTime;
        maxwidth = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)).x - 0.5f;
        PlutoComm.OnButtonReleased += onPlutoButtonReleased;
        randomTargetIndex = random.Next(1, 11);
    }
    private float ScreenPositionToAngle(float screenPosition)
    {
        float calibAngleRange = PlutoComm.CALIBANGLE[PlutoComm.mechanism];
        float angle = Mathf.Lerp(
            -calibAngleRange / 2, // Minimum angle
            calibAngleRange / 2,  // Maximum angle
            (screenPosition + playSize) / (2 * playSize) // Normalized position
        );
        return angle;
    }
    private void UpdateText()
    {
        timeLeftText.text = $"Time Left: {(int)timeLeft}";
        ScoreText.text = $"Score: {gameData.gameScore}";
    }

    private void StartNewGameSession()
    {
        currentGameSession = new GameSession
        {
            GameName = "HAT-Trick",
            Assessment = 0
        };

        SessionManager.Instance.StartGameSession(currentGameSession);
        AppLogger.LogInfo($"Game session {currentGameSession.SessionNumber} started.");
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

    private void EndCurrentGameSession()
    {
        if (currentGameSession != null)
        {
            SessionManager.Instance.SetTrialDataFileLocation(AppData.trialDataFileLocation, currentGameSession);
            SessionManager.Instance.moveTime(gameData.moveTime.ToString("F0"), currentGameSession);
            SessionManager.Instance.gameSpeed(gameData.gameSpeedHT, currentGameSession);
            SessionManager.Instance.successRate(gameData.successRate, currentGameSession);
            SessionManager.Instance.EndGameSession(currentGameSession);
        }
    }
    public void exitGame()
    {
        EndCurrentGameSession();
        SceneManager.LoadScene("choosegame");
    }
    private void onPlutoButtonReleased()
    {
        isPressed = true;
    }

   


}
