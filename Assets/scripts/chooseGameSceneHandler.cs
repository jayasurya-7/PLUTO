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
        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
        AppLogger.LogInfo($"{SceneManager.GetActiveScene().name} scene started.");
        AppLogger.SetCurrentGame("");
        AppData.UserData.CalculateGameSpeedForLastUsageDay();
        PlutoComm.OnButtonReleased += OnPlutoButtonReleased;
        AttachToggleListeners();
        playButton.onClick.AddListener(OnPlayButtonClicked);
        changeMech.onClick.AddListener(OnMechButtonClicked);
        AppData.oldAROM=new AROM(AppData.selectedMechanism);
    }
    void Update()
    {   
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
       // Debug.Log("device ang: " + PlutoComm.angle);
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
