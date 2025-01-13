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
            AppData.selectedMechanism = "HOC";
            AppLogger.SetCurrentMechanism(AppData.selectedMechanism);
        }
        btnStartStop.onClick.AddListener(delegate { OnStartStopDemo(); });
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

        Debug.Log("device ang: " + PlutoComm.angle);
        // Check if the demo is running.
        if (isRunning == false) return;

        // Update trial time
        trialDuration += Time.deltaTime;

        // Run trial state machine
        RunTrialStateMachine();
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
    private void OnStartStopDemo()
    {
        if (isRunning)
        {
            btnStartStop.GetComponentInChildren<TMP_Text>().text = "Start Demo";
            isRunning = false;
            // Stop control.
            PlutoComm.setControlType("NONE");
        }
        else
        {
            // Pluto AAN controller
            aanCtrler = new PlutoAANController();
            // Change button text
            btnStartStop.GetComponentInChildren<TMP_Text>().text = "Stop Demo";
            isRunning = true;
            // Set Control mode.
            PlutoComm.setControlType("POSITIONAAN");
            PlutoComm.setControlBound(currControlBound);
            PlutoComm.setControlDir(0);
            trialNo = 0;
            //successRate = 0;
            // Start the state machine.
            SetTrialState(DiscreteMovementTrialState.Rest);
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
                }
                break;
            case DiscreteMovementTrialState.SetTarget:
                if (_statetimeout)
                {
                    SetTrialState(DiscreteMovementTrialState.Moving);
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
                
                // Reset target timer (for display purposes).
                _tempIntraStateTimer = 0f;
                break;
            case DiscreteMovementTrialState.SetTarget:
                // Random select target from the appropriate range.
                float _tgtscale = UnityEngine.Random.Range(0.0f, 1.0f);
                _trialTarget = -51.0f;
                break;
            case DiscreteMovementTrialState.Moving:
                // Start the position control to the tatget location.
                _initialTarget = PlutoComm.angle;
                _finalTarget = _trialTarget;
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
                PlutoComm.setControlType("NONE");

                //aanCtrler.upateTrialResult(true);
                // Update adaptation row.
                // WriteTrialRowInfo(1);
                break;
            case DiscreteMovementTrialState.Failure:
                //aanCtrler.upateTrialResult(false);
                Debug.Log("Failure");
                isRunning = false;
                PlutoComm.setControlType("NONE");
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
        //// Implement the minimum jerk trajectory.
        //float _t = (trialDuration - stateStartTime) / cbChangeDuration;
        //// Limit _t between 0 and 1.
        //_t = Mathf.Clamp(_t, 0, 1);
        //// Compute the CB value using the minimum jerk trajectory.
        //_currCBforDisplay = prevControlBound + (currControlBound - prevControlBound) * (10 * Mathf.Pow(_t, 3) - 15 * Mathf.Pow(_t, 4) + 6 * Mathf.Pow(_t, 5));
        //// Update control bound.
        ////PlutoComm.setControlBound(_currCBforDisplay);
        //Debug.Log("_curr :" + _currCBforDisplay);
        //Debug.Log("_curr :" + _t);
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

    private void OnMechButtonClicked()
    {
        SceneManager.LoadScene(changeScene);
     
    }

    private void LoadSelectedGameScene(string game)
    {
        if (gameScenes.TryGetValue(game, out string sceneName))
        {
            Debug.Log("Scene name:"+ sceneName);
            if (AppData.selectedMechanism != "HOC") { 
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
