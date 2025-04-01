using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static AppData;
using System;
using Unity.VisualScripting;

public class calibrationSceneHandler : MonoBehaviour
{
    public TextMeshProUGUI textMessage;
    public TextMeshProUGUI mechText;
    public TextMeshProUGUI angText;
    public Button exit;
    private bool startCalibration = false;
    private bool isCalibrating = false;
    private bool doneCalibration = false;
    private string prevScene = "chooseMechanism";
    private string nextScene = "chooseGame";

    void Start()
    {
        // Check if user is not initialized.

        // Set mechanism to NOMECH.
        PlutoComm.sendHeartbeat();
        // Set mechanism to the selected mechanism.
        PlutoComm.calibrate(AppData.selectedMechanism);

        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
        AppLogger.LogInfo($"'{SceneManager.GetActiveScene().name}' scene started.");
        mechText.text = PlutoComm.MECHANISMSTEXT[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, selectedMechanism)];

        // Attach callback.
        if (ConnectToRobot.isPLUTO)
        {
            PlutoComm.OnButtonReleased += OnPlutoButtonReleased;
        }
        exit.onClick.AddListener(OnExitButtonClicked);
    }

    void Update()
    {
        PlutoComm.sendHeartbeat();
        angText.text = $"{PlutoComm.angle.ToString("F3")} + {PlutoComm.CONTROLTYPETEXT[PlutoComm.controlType]}+{PlutoComm.MECHANISMS[PlutoComm.mechanism]}";
        // Check of calibration is started.
        if (!isCalibrating && startCalibration)
        {
            PerformCalibration();
            startCalibration = false;
        }
    }

    private void PerformCalibration()
    {
        if (string.IsNullOrEmpty(selectedMechanism))
        {
            Debug.LogError("No mechanism selected for calibration!");
            return;
        }

        // Start the calibration process.
        StartCoroutine(autoCalibrate());
    }

    IEnumerator autoCalibrate()
    {
        textMessage.color = Color.black;
        textMessage.text = "Calibrating...";
        PlutoComm.setControlType("TORQUE");
        // Move the robot to the extreme position.
        ApplyCounterClockwiseTorque();
        AppLogger.LogInfo($"controlType : {PlutoComm.controlType} : applying first torque in counter clockwise");
        yield return new WaitForSeconds(0.8f);

        // Send the calibration command.
        PlutoComm.calibrate(AppData.selectedMechanism);
        AppLogger.LogInfo($"controlType : {PlutoComm.MECHANISMS[PlutoComm.mechanism]} : calibrated mechanism");
        yield return new WaitForSeconds(0.3f);

        //ApplyTorqueToSep(PlutoComm.angle, separationAngle);
        ApplyClockwiseTorque();
        AppLogger.LogInfo($"controlType : {PlutoComm.controlType} : applying second torque clockwise");
        yield return new WaitForSeconds(0.8f);

        // Check if the ROM is correct.
        int mechInx = Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism);
        float _angval = PlutoComm.angle + PlutoComm.MECHOFFSETVALUE[mechInx];
        isCalibrating = false;
        if (Math.Abs(_angval) < 0.9 * PlutoComm.CALIBANGLE[mechInx]
            || Math.Abs(_angval) > 1.1 * PlutoComm.CALIBANGLE[mechInx])
        {
            // Error in calibration
            PlutoComm.setControlType("NONE");
            //PlutoComm.calibrate("NOMECH");
            textMessage.text = $"Try Again.";
            textMessage.color = Color.red;
            AppLogger.LogError($"Calibration failed for {AppData.selectedMechanism}.");
            isCalibrating = false;
            doneCalibration = false;
            yield break;
        }
        // All good.
        textMessage.text = "Calibration Done";
        textMessage.color = new Color32(62, 214, 111, 255);
        AppLogger.LogError($"Calibration was successful for {AppData.selectedMechanism}.");

        //HOC assessment UI  works based on closed position,
        if (PlutoComm.MECHANISMS[PlutoComm.mechanism] != "HOC")
        {
            // Move the robot to the neutral position.
            PlutoComm.setControlType("POSITION");
            // Set the target to zero slowly.
            float _initAngle = PlutoComm.angle;
            int N = 20;
            for (int i = 0; i < N; i++)
            {
                PlutoComm.setControlBound(1.0f * (i + 1) / N);
                PlutoComm.setControlTarget((N - i) * _initAngle / N);
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (PlutoComm.MECHANISMS[PlutoComm.mechanism] == "HOC") PlutoComm.calibrate(AppData.selectedMechanism);

        PlutoComm.setControlTarget(0.0f);
        PlutoComm.setControlType("NONE");
        yield return new WaitForSeconds(1.5f);

        // Set selected mechanism.
        AppData.selectedMechanism = PlutoComm.MECHANISMS[PlutoComm.mechanism];
        AppLogger.SetCurrentMechanism(AppData.selectedMechanism);

        // Update flags.
        isCalibrating = false;
        doneCalibration = true;

        // Go to the next scene.
        Invoke("LoadNextScene", 0.4f);
    }

    void LoadNextScene()
    {
        AppLogger.LogInfo($"Switching scene to '{nextScene}'.");
        SceneManager.LoadScene(nextScene);
    }

    private void ApplyCounterClockwiseTorque()
    {
        float torqueValue = -0.1f;
      //  PlutoComm.setControlType("TORQUE");
        PlutoComm.setControlTarget(torqueValue);
    }

    private void ApplyClockwiseTorque()
    {
        float torqueValue = 0.1f;
        // PlutoComm.setControlType("TORQUE");
        PlutoComm.setControlTarget(torqueValue);
    }

    private void OnPlutoButtonReleased()
    {
        if (!doneCalibration && !isCalibrating && !startCalibration)
        {
            startCalibration = true;
        }
    }

    private void OnExitButtonClicked()
    {
        SceneManager.LoadScene(prevScene);
    }

    private void OnDestroy()
    {
        if (ConnectToRobot.isPLUTO)
        {
            PlutoComm.OnButtonReleased -= OnPlutoButtonReleased;
        }
    }
}



//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using static AppData;

//public class calibrationSceneHandler : MonoBehaviour
//{
//    private string selectedMechanism;
//    private bool isCalibrating = false;
//    private float togetherPosition = 0.0f;
//    private float togetherAngle = 0f;
//    private float separationPosition = 11.0f;
//    private float separationAngle = 180.0f;
//    private float separationAngleWFE = 140.0f;
//    public TextMeshProUGUI textMessage;
//    public TextMeshProUGUI mechText;
//    public TextMeshProUGUI angText;
//    private static bool connect = false;
//    public Button exit;
//    private string prevScene = "chooseMechanism";
//    private string nextScene = "choosegame";



//    void Start()
//    {

//        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
//        AppLogger.LogInfo($"{SceneManager.GetActiveScene().name} scene started.");
//        selectedMechanism = AppData.selectedMechanism;
//        mechText.text = PlutoComm.MECHANISMSTEXT[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, selectedMechanism)];
//        exit.onClick.AddListener(OnExitButtonClicked);
//    }

//    void Update()
//    {
//        PlutoComm.sendHeartbeat();
//        if (Input.GetKeyDown(KeyCode.C) && !isCalibrating)
//        {
//            PerformCalibration();
//        }

//        if (ConnectToRobot.isPLUTO)
//        {
//            PlutoComm.OnButtonReleased += OnPlutoButtonReleased;
//        }

//        if (isCalibrating)
//        {
//            PerformCalibration();
//            isCalibrating = false;
//        }
//        angText.text = PlutoComm.angle.ToString("F3");
//    }

//    private void PerformCalibration()
//    {
//        if (string.IsNullOrEmpty(selectedMechanism))
//        {
//            Debug.LogError("No mechanism selected for calibration!");
//            return;
//        }

//        switch (selectedMechanism)
//        {
//            case "HOC":
//                StartCoroutine(autoCalibrateHOC());
//                break;

//            case "WFE":
//            case "WURD":
//                StartCoroutine(autoCalibrate(togetherAngle, separationAngleWFE));
//                break;

//            case "FPS":
//                StartCoroutine(autoCalibrate(togetherAngle, separationAngle));
//                break;

//            case "FME1":
//            case "FME2":
//                StartCoroutine(autoCalibrate(togetherAngle, separationAngle));
//                break;

//            default:
//                Debug.LogError("Unknown mechanism type selected: " + selectedMechanism);
//                break;
//        }
//    }


//    IEnumerator autoCalibrateHOC()
//    {

//        textMessage.color = Color.black;
//        textMessage.text = "Calibrating...";

//        float currentDistance = PlutoComm.getHOCDisplay(PlutoComm.angle);
//        ApplyTorqueToSep(currentDistance, togetherPosition);
//        yield return new WaitForSeconds(1.5f);


//        PlutoComm.calibrate(selectedMechanism);

//        ApplyTorque(PlutoComm.getHOCDisplay(PlutoComm.angle), separationPosition);

//        yield return new WaitForSeconds(1.5f);

//        if (!CheckPositionSeparation(PlutoComm.getHOCDisplay(PlutoComm.angle), separationPosition)) yield break;

//        ApplyTorqueToSep(PlutoComm.getHOCDisplay(PlutoComm.angle), togetherPosition);

//        yield return new WaitForSeconds(1.5f);
//        if (!CheckPositionTogether(PlutoComm.getHOCDisplay(PlutoComm.angle), togetherPosition)) yield break;
//        textMsg();
//        Invoke("LoadNextScene", 0.4f);
//    }

//    IEnumerator autoCalibrate(float togetherAngle, float separationAngle)
//    {
//        textMessage.color = Color.black;
//        textMessage.text = "Calibrating...";

//        float currentAngle = PlutoComm.angle;
//        float temp0 = -90f;
//        float temp1 = 90f;
//        // ApplyTorque(currentAngle, togetherAngle);
//        ApplyTorque(currentAngle, temp0);
//        yield return new WaitForSeconds(1.5f);

//        PlutoComm.calibrate(AppData.selectedMechanism);

//        //ApplyTorqueToSep(PlutoComm.angle, separationAngle);
//        ApplyTorqueToSep(PlutoComm.angle, temp1);

//        yield return new WaitForSeconds(1.5f);
//        if (!CheckPositionSeparation(PlutoComm.angle, temp1)) yield break;
//        ApplyTorque(PlutoComm.angle, temp0);


//        yield return new WaitForSeconds(1.5f);

//        if (!CheckPositionTogether(PlutoComm.angle, temp0)) yield break;
//        textMsg();
//        //HOC assessment UI  works based on closed position,
//        if (PlutoComm.MECHANISMS[PlutoComm.mechanism] != "HOC")
//        {
//            // Move the robot to the neutral position.
//            PlutoComm.setControlType("POSITION");
//            // Set the target to zero slowly.
//            float _initAngle = PlutoComm.angle;
//            int N = 20;
//            for (int i = 0; i < N; i++)
//            {
//                PlutoComm.setControlBound(1.0f * (i + 1) / N);
//                PlutoComm.setControlTarget((N - i) * _initAngle / N);
//                yield return new WaitForSeconds(0.1f);
//            }
//        }

//        Invoke("LoadNextScene", 0.4f);
//    }

//    void LoadNextScene()
//    {
//        AppLogger.LogInfo($"Switching scene to '{nextScene}'.");
//        PlutoComm.setControlType("NONE");
//        SceneManager.LoadScene(nextScene);
//    }
//    private void ApplyTorque(float currentPos, float targetPos)
//    {
//        float torqueValue = -0.1f;
//        PlutoComm.setControlType("TORQUE");
//        PlutoComm.setControlTarget(torqueValue);
//    }
//    private void ApplyTorqueToSep(float currentPos, float targetPos)
//    {
//        float torqueValue = 0.1f;
//        PlutoComm.setControlType("TORQUE");
//        PlutoComm.setControlTarget(torqueValue);
//    }

//    private void OnPlutoButtonReleased()
//    {
//        isCalibrating = true;
//    }


//    private bool CheckPositionTogether(float currentPosition, float targetPosition)
//    {
//        float targetPos = targetPosition + 1.5f;
//        if (currentPosition <= targetPos)
//        {
//            return true;
//        }
//        else
//        {
//            errMsg();
//            return false;
//        }
//    }

//    private bool CheckPositionSeparation(float currentPosition, float targetPosition)
//    {
//        if (selectedMechanism == "HOC")
//        {
//            float targetPos = targetPosition - 3f;
//            if (currentPosition >= targetPos)
//            {
//                return true;
//            }
//            else
//            {
//                errMsg();
//                return false;
//            }
//        }
//        else
//        {
//            float targetPos = targetPosition - 2f;
//            if (currentPosition >= targetPos)
//            {
//                return true;
//            }
//            else
//            {
//                errMsg();
//                return false;
//            }
//        }

//    }

//    private void errMsg()
//    {
//        textMessage.text = $"Try Again.";
//        textMessage.color = Color.red;
//        isCalibrating = false;
//        PlutoComm.calibrate(AppData.selectedMechanism);
//        PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
//    }

//    private void textMsg()
//    {
//        isCalibrating = false;
//        textMessage.text = "Calibration Done";
//        textMessage.color = new Color32(62, 214, 111, 255);
//        PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
//    }
//    private void OnExitButtonClicked()
//    {
//        SceneManager.LoadScene(prevScene);
//    }

//    private void OnDestroy()
//    {
//        if (ConnectToRobot.isPLUTO)
//        {
//            PlutoComm.OnButtonReleased -= OnPlutoButtonReleased;
//        }
//    }
//}