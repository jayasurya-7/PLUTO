
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
    private float trialTime = 60f;
    private float timeLeft;
    public bool balldestroyed = true;
    private bool isPressed = false;
    private bool isPaused = false;
    private int count;
    private float x;
    private float targetAngle;

    private GameSession currentGameSession;

    private bool isPlaying = false; 
    private float Player;
    private sbyte direction;
    private enum GameState { NotStarted, Playing, Paused, GameOver }
    private GameState currentState = GameState.NotStarted;

    private enum DiscreteMovementTrialState { Rest,Moving }
    private DiscreteMovementTrialState trialState = DiscreteMovementTrialState.Rest;

    private float targetPosition;
    private float playerPosition;
    private bool isRunning = false;
    private float _initialTarget = 0;
    private float _finalTarget = 0;
    public bool targetSpwan= false;

    private int outsideAromRangeCount = 0; 
    private int totalTargetsSpawned = 0;

    private float prevControlBound = 0.16f;
    // Magical minimum value where the mechanisms mostly move without too much instability.
    private float currControlBound = 0.16f;

    private DiscreteMovementTrialState _trialState;
    private const float tgtHoldDuration = 1f;
    private float _trialTarget = 0f;
   
    private float trialDuration = 0f;
    public bool aromRangeSpawn=false;
    public Toggle spawnAreaToggle; 
    //private int successRate;
    public Image targetImage; 
    private int randomTargetIndex;
    private int spawnCounter = 0;
    private System.Random random = new System.Random();
    private PlutoAANController aanCtrler;
    public bool IsPlaying 
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



        if (PlutoComm.CONTROLTYPE[PlutoComm.controlType] == "NONE" && !aromRangeSpawn) {
            PlutoComm.setControlType("POSITIONAAN");
            Debug.Log("AAN applied");
        }
        if (currentState == GameState.Playing)
        {
            HandleGameUpdate();
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position.x;
          
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
        if (aromRangeSpawn) return;
        RunTrialStateMachine();
        if (_trialState == DiscreteMovementTrialState.Moving)
        {
            trialDuration += Time.deltaTime;
        }
       //  Debug.Log("im running");
        //Player = GameObject.FindGameObjectWithTag("Player").transform.position.x;
        //Debug.Log("controlType :" + PlutoComm.CONTROLTYPE[PlutoComm.controlType]);
    }


  private void RunTrialStateMachine()
{
    trialDuration += Time.deltaTime;

    switch (_trialState)
    {
        case DiscreteMovementTrialState.Rest:
            if (targetSpwan && trialDuration >= 0.25f)
            {
                SetTrialState(DiscreteMovementTrialState.Moving);
            }
            break;

        case DiscreteMovementTrialState.Moving:
                if (targetSpwan)
                {
                    UpdateControlBoundSmoothly();
                    UpdatePositionTargetSmoothly();

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
            targetSpwan = false; 
            break;

        case DiscreteMovementTrialState.Moving:
            trialDuration = 0f;
            _initialTarget = PlutoComm.angle;
            _finalTarget = targetAngle;
            PlutoComm.setControlDir((sbyte)(targetPosition > playerPosition ? 1 : -1));

            aanCtrler.setNewTrialDetails(_initialTarget, _finalTarget);
            break;
    }
}

    private float SpawnTargetArea()
    {
        AppData.newAROM = new AROM(AppData.selectedMechanism);
        float aromMin = AppData.newAROM.tmin; 
        float aromMax = AppData.newAROM.tmax; 

        float xMin = MapAROMToPROMPlaySize(aromMin);
        float xMax = MapAROMToPROMPlaySize(aromMax);

        float targetPosition = UnityEngine.Random.Range(xMin, xMax);

        Debug.Log($"Spawned Target Area Position: {targetPosition} (AROM Min: {aromMin}, Max: {aromMax}, Mapped X Min: {xMin}, Mapped X Max: {xMax})");
        return targetPosition;
    }
    private float MapAROMToPROMPlaySize(float angle)
    {
        AppData.newPROM = new MechanismData(AppData.selectedMechanism);
        float promMin = AppData.newPROM.tmin; 
        float promMax = AppData.newPROM.tmax; 
        float promRange = promMax - promMin;
        float normalizedAROM = (angle - promMin) / promRange;

        
        float scalingFactor = 0.5f; 
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

    private void OnToggleSpawnArea(bool isEnabled)
    {
        aromRangeSpawn = isEnabled;
        PlutoComm.setControlType("NONE");
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
            totalTargetsSpawned++;

            if (aromRangeSpawn)
            {
                if (outsideAromRangeCount < 2 && totalTargetsSpawned % 10 <= 1)
                {
                    targetPosition = UnityEngine.Random.Range(-playSize * 1.5f, playSize * 1.5f); 

                    Debug.Log(targetPosition);
                    outsideAromRangeCount++;
                }
                else
                {
                    targetPosition = SpawnTargetArea();
                }
            }
            else
            {
                
                targetPosition = UnityEngine.Random.Range(-playSize + 0.5f, playSize - 0.5f);

            }
            Vector3 spawnPosition = new Vector3(targetPosition, 6f, 0);
            Quaternion spawnRotation = Quaternion.identity;

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
            targetAngle = ScreenPositionToAngle(targetPosition); 
            if (totalTargetsSpawned == randomTargetIndex)
            {
                targetImage.gameObject.SetActive(true); 
                Debug.Log("Displaying the target image!");
            }
            else
            {
                targetImage.gameObject.SetActive(false); 
            }
        }
    }

    private void UpdateControlBoundSmoothly()
{
    if (!targetSpwan) return;
    float t = trialDuration / 4.5f; 
    float smoothedControlBound = Mathf.Lerp(0f, 0.6f, t);
    PlutoComm.setControlBound(smoothedControlBound);
}
private void UpdatePositionTargetSmoothly()
{
    float t = trialDuration / 4.5f;
    float smoothedTargetPosition = Mathf.Lerp(_initialTarget, _finalTarget, t);
    PlutoComm.setControlTarget(smoothedTargetPosition);
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
        Debug.Log("Random Target:" + randomTargetIndex);
    }
    private float ScreenPositionToAngle(float screenPosition)
    {
        float calibAngleRange = PlutoComm.CALIBANGLE[PlutoComm.mechanism];
        float angle = Mathf.Lerp(
            -calibAngleRange / 2, 
            calibAngleRange / 2,  
            (screenPosition + playSize) / (2 * playSize) 
        );
        return angle;
    }
    private void UpdateText()
    {
        timeLeftText.text = $"Time Left: {(int)timeLeft}";
        ScoreText.text = $"Score: {gameData.gameScore}";
        if (gameData.gameScore > 0 && gameData.gameScore < 11)
        {
            gameData.successRate = (float)gameData.gameScore / 10;
        }
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
