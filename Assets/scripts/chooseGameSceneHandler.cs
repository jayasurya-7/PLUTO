using System;
using UnityEngine;
using UnityEngine.SceneManagement;  
using UnityEngine.UI;  
using System.Collections.Generic; 
using System.Collections;
using TMPro;
using UnityEditor.U2D.Aseprite;

public class ChooseGameSceneHandler : MonoBehaviour
{
    public GameObject toggleGroup;  
    public Button playButton;   
    public Button changeMech;
    public TMP_Text result;

    private bool toggleSelected = false;  
    private string selectedGame;
    private string changeScene = "chooseMechanism";
    private static bool isButtonPressed = false;
    private readonly Dictionary<string, string> gameScenes = new Dictionary<string, string>
    {
        { "pingPong", "pong_menu" },
        { "tukTuk", "FlappyGame" },
        { "hatTrick", "HatrickGame" }
    };
    //AAN
    private bool lisRunning = false; // Tracks if the automatic movement is running
   // private float targetAngle = 50.0f; // The target angle to reach
    private bool targetReached = false; // Tracks if the target is reached

    private const float targetTolerance = 10.0f; // Tolerance for reaching the target


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
        2.50f,          // Rest duration
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
    public Button btnStartStop;

    private float targetAngle = 0;
    // AAN class
    private PlutoAANController aanCtrler;
    void Start()
    {

        // Initialize if needed
        if (AppData.UserData.dTableConfig == null)
        {
            // Inialize the logger
            AppLogger.StartLogging(SceneManager.GetActiveScene().name);
            // Initialize.
            AppData.initializeStuff();
            AppData.selectedMechanism = "FPS";
            AppLogger.SetCurrentMechanism(AppData.selectedMechanism);
        }
        AppData.oldPROM = new MechanismData(AppData.selectedMechanism);
        //targetAngle = (AppData.oldPROM.tmax + AppData.oldPROM.tmin)/2;
        targetAngle= AppData.offsetAtNeutral[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS,AppData.selectedMechanism)];
        Debug.Log("PROM : " + AppData.oldPROM.tmax + " + " + AppData.oldPROM.tmin);
        // btnStartStop.onClick.AddListener(delegate { OnStartStopDemo(); });
        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
        AppLogger.LogInfo($"{SceneManager.GetActiveScene().name} scene started.");
        AppLogger.SetCurrentGame("");
        AppData.UserData.CalculateGameSpeedForLastUsageDay();
        PlutoComm.OnButtonReleased += OnPlutoButtonReleased;
        AttachToggleListeners();
        PlutoComm.setControlType("NONE");
        playButton.onClick.AddListener(OnPlayButtonClicked);
        changeMech.onClick.AddListener(OnMechButtonClicked);
        AppData.oldAROM=new AROM(AppData.selectedMechanism);

        // Start the mechanism movement after a delay of 1 second
        StartCoroutine(SetMechanismToTargetAfterDelay(1.0f));

    }
    void Update()
    {   
        PlutoComm.sendHeartbeat();
        if (isButtonPressed)
        {
            LoadSelectedGameScene(selectedGame);
            isButtonPressed = false;
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            //assessment();
            SceneManager.LoadScene("Assessment");

        }

        // Monitor the mechanism's progress if it's running
        if (isRunning && !targetReached)
        {
            float currentAngle = PlutoComm.angle;

            // Check if the mechanism has reached the target
            if (Mathf.Abs(currentAngle - targetAngle) <= targetTolerance)
            {
                targetReached = true;
                isRunning = false;

                // Set control type to NONE after reaching the target
                PlutoComm.setControlType("NONE");
                Debug.Log($"Target reached: {currentAngle}. Control type set to NONE.");
            }
        }
    }

    private IEnumerator SetMechanismToTargetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Start moving the mechanism to the target angle
        PlutoComm.setControlType("POSITIONAAN");
        PlutoComm.setControlBound(0.2f);
        PlutoComm.setControlTarget(targetAngle);
        isRunning = true;

        Debug.Log($"Started moving mechanism to {targetAngle} degrees.");
    }

    void AttachToggleListeners()
    {
        foreach (Transform child in toggleGroup.transform)
        {
            Toggle toggleComponent = child.GetComponent<Toggle>();
            if (toggleComponent != null)
            {
                toggleComponent.onValueChanged.AddListener(delegate { CheckToggleStates(); });
            }
        }
    }

    void CheckToggleStates()
    { 
        foreach (Transform child in toggleGroup.transform)
        {
            Toggle toggleComponent = child.GetComponent<Toggle>();
            if (toggleComponent != null && toggleComponent.isOn)
            {
                selectedGame = toggleComponent.name;  
                AppData.selectedGame = selectedGame;
                AppLogger.SetCurrentGame(AppData.selectedGame);
                AppLogger.LogInfo($"Selected game '{AppData.selectedGame}'.");
                toggleSelected = true; 
                break; 
            }
        }
    }

    private void OnPlayButtonClicked()
    {
        if (toggleSelected)
        {
            LoadSelectedGameScene(selectedGame);
            toggleSelected = false;
        }
        else
        {
            Debug.Log("No game selected. Please select a game.");
        }
    }





   
    private void OnMechButtonClicked()
    {
        SceneManager.LoadScene(changeScene);
     
    }

    private void LoadSelectedGameScene(string game)
    {
        if (gameScenes.TryGetValue(game, out string sceneName))
        {
            Debug.Log("Scene name:"+ sceneName);
            if (AppData.selectedMechanism != "HOC")
            {
                PlutoComm.calibrate(AppData.selectedMechanism); //its temp, needs to set 0 using control type 
            }
            SceneManager.LoadScene(sceneName);
        }
    }
    public void OnPlutoButtonReleased()
    {
        if (toggleSelected)
        {
            isButtonPressed=true;
            toggleSelected = false;
        }
        else
        {
            Debug.Log("No game selected. Please select a game.");
        }
    }
    private void assessment()
    {
        string date = AppData.oldAROM.datetime; 
        Debug.Log($"AppData.oldAROM.datetime: {date}");

        if (!string.IsNullOrEmpty(date))
        {
            DateTime oldDate;
            if (DateTime.TryParseExact(date, "dd-MM-yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out oldDate))
            {  
                DateTime currentDate = DateTime.Now;
                TimeSpan timeDifference = currentDate - oldDate;

                result.text = $"Current Date: {currentDate}, Old Date: {oldDate}, Days Passed: {timeDifference.TotalDays:F2}";

                if (timeDifference.TotalDays >= 7)
                {
                    SceneManager.LoadScene("Assessment"); 
                }
                else
                {
                    Debug.Log($"Only {timeDifference.TotalDays} days have passed. 7 days required.");
                }
            }
            else
            {
                Debug.LogError($"Invalid date format: {date}. Expected format: 'dd-MM-yyyy HH:mm:ss'.");
            }
        }
        else
        {
            Debug.LogError("Date is null or empty.");
        }
    }

    private void OnDestroy()
    {
        if (ConnectToRobot.isPLUTO)
        {
            PlutoComm.OnButtonReleased -= OnPlutoButtonReleased;
        }
    }

}
