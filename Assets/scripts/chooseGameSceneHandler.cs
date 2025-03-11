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
    private static bool isButtonPressed = false;
    private readonly Dictionary<string, string> gameScenes = new Dictionary<string, string>
    {
        { "pingPong", "pong_menu" },
        { "tukTuk", "FlappyGame" },
        { "hatTrick", "HatrickGame" }
    };
    private bool lisRunning = false;
    private bool targetReached = false; 
    private const float targetTolerance = 5.0f; 
    private bool isRunning = false;
    
    private float targetAngle = 0;
    private string assessmentScene = "assessment";
    private string changeScene = "chooseMechanism";

    void Start()
    {
        PlutoComm.setControlType("NONE");

        initialize();

        //applogger
        applogging();

        //calculate game speed
        AppData.UserData.CalculateGameSpeedForLastUsageDay();

        PlutoComm.OnButtonReleased += OnPlutoButtonReleased;

        ROM romValues = new ROM(AppData.selectedMechanism);

        AttachToggleListeners();
        
        playButton.onClick.AddListener(OnPlayButtonClicked);
        changeMech.onClick.AddListener(OnMechButtonClicked);

        if(romValues.datetime == null) SceneManager.LoadScene(assessmentScene);

        AppData.aRomValue[0] = romValues.aromTmin;
        AppData.aRomValue[1] = romValues.aromTmax;
        AppData.pRomValue[0] = romValues.promTmin;
        AppData.pRomValue[1] = romValues.promTmax;

        //set mechanism to neutral position
        if (!gameData.setNeutral)
        {
            StartCoroutine(SetMechanismToTargetAfterDelay(1.0f));
        }
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
           SceneManager.LoadScene(assessmentScene);
        }
        //assessment(); //automatic assessment scene load when 7 days done.

        if (isRunning && !targetReached)
        {
            float currentAngle = PlutoComm.angle;
            if (Mathf.Abs(currentAngle - 0f) <= targetTolerance) //for now,its 0 in future we need to change according to mech.
            {
                targetReached = true;
                isRunning = false;
                gameData.setNeutral = true;
                PlutoComm.setControlType("NONE");
                Debug.Log($"Target reached: {currentAngle}. Control type set to NONE.");
            }
        }
    }


    private IEnumerator SetMechanismToTargetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        PlutoComm.setControlType("POSITIONAAN");
        PlutoComm.setControlBound(0.9f);
        PlutoComm.setControlDir(1);
        PlutoComm.setAANTarget(PlutoComm.angle, 0f, 0f, 2f);
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


    private void initialize()
    {
        // Initialize if needed
        if (AppData.UserData.dTableConfig == null)
        {
            // Inialize the logger
            AppLogger.StartLogging(SceneManager.GetActiveScene().name);
            AppData.initializeStuff();
            AppData.selectedMechanism = "WFE";
            AppData.currentSessionNumber = 1111;
            AppData.runIndividualGame = true;
            AppLogger.SetCurrentMechanism(AppData.selectedMechanism);
        }
    }

    private void applogging()
    {

        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
        AppLogger.LogInfo($"{SceneManager.GetActiveScene().name} scene started.");
        AppLogger.SetCurrentGame("");
    }

   
    private void OnMechButtonClicked()
    {
        SceneManager.LoadScene(changeScene);
     
    }

    private void LoadSelectedGameScene(string game)
    {
        if (gameScenes.TryGetValue(game, out string sceneName))
        {
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
       // Debug.Log($"AppData.oldAROM.datetime: {date}");

        if (!string.IsNullOrEmpty(date))
        {
            DateTime oldDate;
            if (DateTime.TryParseExact(date, "dd-MM-yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out oldDate))
            {  
                DateTime currentDate = DateTime.Now;
                TimeSpan timeDifference = currentDate - oldDate;

                result.text = $"Current Date: {currentDate}, Old Date: {oldDate}, Days Passed: {timeDifference.TotalDays:F1}";

                if (timeDifference.TotalDays >= 7)
                {
                    SceneManager.LoadScene("Assessment"); 
                }
                else
                {
                   // Debug.Log($"Only {timeDifference.TotalDays:F1} days have passed. 7 days required.");
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
