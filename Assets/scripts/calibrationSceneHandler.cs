using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static AppData;

public class calibrationSceneHandler : MonoBehaviour
{
    private string selectedMechanism;
    private bool isCalibrating = false;
    private float togetherPosition = 0.0f;
    private float togetherAngle = 0f;

    private float separationPosition = 11.0f;
    private float separationAngle = 22.0f;
    public TextMeshProUGUI textMessage;
    public TextMeshProUGUI mechText;
    private static bool connect = false;
    public Button exit;
    private string prevScene = "chooseMechanism";
    private string nextScene = "choosegame";



    void Start()
    {
        AppLogger.SetCurrentScene(SceneManager.GetActiveScene().name);
        AppLogger.LogInfo($"{SceneManager.GetActiveScene().name} scene started.");
        selectedMechanism = AppData.selectedMechanism;
        int mechNumber = PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, selectedMechanism);
        mechText.text = PlutoComm.MECHANISMSTEXT[mechNumber];
        exit.onClick.AddListener(OnExitButtonClicked);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isCalibrating)
        {
            PerformCalibration();
        }

        if (ConnectToRobot.isPLUTO)
        {
            PlutoComm.OnButtonReleased += OnPlutoButtonReleased;

        }

        if (isCalibrating)
        {
            PerformCalibration();
            isCalibrating = false;
        }
    }

    private void PerformCalibration()
    {
        if (string.IsNullOrEmpty(selectedMechanism))
        {
            Debug.LogError("No mechanism selected for calibration!");
            return;
        }

        switch (selectedMechanism)
        {
            case "HOC":
                StartCoroutine(autoCalibrateHOC());
                break;

            case "WFE":
                StartCoroutine(autoCalibrateWFEandWURD());
                break;

            case "WURD":
                ;
                StartCoroutine(autoCalibrateWFEandWURD());
                break;

            case "FPS":
                StartCoroutine(autoCalibrateFPS());
                break;

            case "FME1":
                StartCoroutine(autoCalibrateFME());
                break;

            case "FME2":
                StartCoroutine(autoCalibrateFME());
                break;

            default:
                Debug.LogError("Unknown mechanism type selected: " + selectedMechanism);
                break;
        }
    }


    IEnumerator autoCalibrateHOC()
    {
        textMessage.color = Color.black;
        textMessage.text = "Calibrating...";

        float currentDistance = PlutoComm.getHOCDisplay(PlutoComm.angle);

        ApplyTorqueToMoveHandles(currentDistance, 0);
        yield return new WaitForSeconds(1.0f);

        float currentDistance1 = PlutoComm.getHOCDisplay(PlutoComm.angle);
        if (!CheckPositionTogether(1, togetherPosition)) yield break;

        PlutoComm.calibrate(selectedMechanism);

        ApplyTorqueToMoveHandles(0, separationPosition);

        yield return new WaitForSeconds(1.0f);
        currentDistance = PlutoComm.getHOCDisplay(PlutoComm.angle);
        if (!CheckPositionSeparation(currentDistance, separationPosition)) yield break;

        ApplyTorqueToMoveHandles(currentDistance, togetherPosition);

        yield return new WaitForSeconds(1.0f);
        currentDistance = PlutoComm.getHOCDisplay(PlutoComm.angle);

        isCalibrating = false;
        textMessage.text = "Calibration Done";
        textMessage.color = new Color32(62, 214, 111, 255);
        PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);

        Invoke("LoadNextScene", 0.4f);
    }

    IEnumerator autoCalibrateFPS()
    {
        textMessage.color = Color.black;
        textMessage.text = "Calibrating...";

        float currentAngle = PlutoComm.getHOCDisplay(PlutoComm.angle);
        Debug.Log("Current Angle when starting:" + currentAngle);

        ApplyTorqueToMoveHandles(currentAngle, togetherAngle);

        yield return new WaitForSeconds(1.0f);

        float currentAngle1 = PlutoComm.getHOCDisplay(PlutoComm.angle);
        Debug.Log("Current Angle when together position" + currentAngle1);

        if (!CheckPositionTogetherAng(1, togetherAngle)) yield break;

        PlutoComm.calibrate(selectedMechanism);

        ApplyTorqueToMoveHandles(currentAngle, separationAngle);
        Debug.Log("Current Angle after separation:" + currentAngle);

        yield return new WaitForSeconds(1.0f);
        currentAngle = PlutoComm.getHOCDisplay(PlutoComm.angle);
        if (!CheckPositionSeparationAng(currentAngle, separationAngle)) yield break;


        ApplyTorqueToMoveHandles(currentAngle, togetherAngle);
        Debug.Log("Current Angle last: " + currentAngle);


        yield return new WaitForSeconds(1.0f);
        currentAngle = PlutoComm.getHOCDisplay(PlutoComm.angle);

        isCalibrating = false;
        textMessage.text = "Calibration Done";
        textMessage.color = new Color32(62, 214, 111, 255);
        PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);

        Invoke("LoadNextScene", 0.4f);
    }

    IEnumerator autoCalibrateWFEandWURD()
    {
        textMessage.color = Color.black;
        textMessage.text = "Calibrating...";

        //float currentAngle = PlutoComm.angle;
        float currentAngle = PlutoComm.getHOCDisplay(PlutoComm.angle);
        Debug.Log("Current Angle when starting:" + currentAngle);

        ApplyTorqueToMoveHandles(currentAngle, togetherAngle);

        yield return new WaitForSeconds(1.0f);

        float currentAngle1 = PlutoComm.getHOCDisplay(PlutoComm.angle);
        Debug.Log("Current Angle when together position" + currentAngle1);

        if (!ChkPosTogetherWFEWURD(1, togetherAngle)) yield break;

        PlutoComm.calibrate(selectedMechanism);

        ApplyTorqueToMoveHandles(currentAngle, 16);
        Debug.Log("Current Angle after separation:" + currentAngle);

        yield return new WaitForSeconds(1.0f);
        currentAngle = PlutoComm.getHOCDisplay(PlutoComm.angle);
        if (!ChkPosSeparationWFEWURD(currentAngle, 16)) yield break;


        ApplyTorqueToMoveHandles(currentAngle, togetherAngle);
        Debug.Log("Current Angle last: " + currentAngle);


        yield return new WaitForSeconds(1.0f);
        currentAngle = PlutoComm.getHOCDisplay(PlutoComm.angle);

        isCalibrating = false;
        textMessage.text = "Calibration Done";
        textMessage.color = new Color32(62, 214, 111, 255);
        PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);

        Invoke("LoadNextScene", 0.4f);
    }


    IEnumerator autoCalibrateFME()
    {
        textMessage.color = Color.black;
        textMessage.text = "Calibrating...";

        //float currentAngle = PlutoComm.angle;
        float currentAngle = PlutoComm.getHOCDisplay(PlutoComm.angle);
        Debug.Log("Current Angle when starting:" + currentAngle);

        ApplyTorqueToMoveHandles(currentAngle, togetherAngle);

        yield return new WaitForSeconds(1.0f);

        float currentAngle1 = PlutoComm.getHOCDisplay(PlutoComm.angle);
        Debug.Log("Current Angle when together position" + currentAngle1);

        if (!ChkPosTogetherWFEWURD(1, togetherAngle)) yield break;

        PlutoComm.calibrate(selectedMechanism);

        ApplyTorqueToMoveHandles(currentAngle, 30);
        Debug.Log("Current Angle after separation:" + currentAngle);

        yield return new WaitForSeconds(1.0f);
        currentAngle = PlutoComm.getHOCDisplay(PlutoComm.angle);
        if (!ChkPosSeparationWFEWURD(currentAngle, 30)) yield break;


        ApplyTorqueToMoveHandles(currentAngle, togetherAngle);
        Debug.Log("Current Angle last: " + currentAngle);


        yield return new WaitForSeconds(1.0f);
        currentAngle = PlutoComm.getHOCDisplay(PlutoComm.angle);

        isCalibrating = false;
        textMessage.text = "Calibration Done";
        textMessage.color = new Color32(62, 214, 111, 255);
        PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);

        Invoke("LoadNextScene", 0.4f);
    }

    void LoadNextScene()
    {
        AppLogger.LogInfo($"Switching scene to '{nextScene}'.");
        SceneManager.LoadScene(nextScene);
    }
    private void ApplyTorqueToMoveHandles(float currentPos, float targetPos)
    {
        float distance = targetPos - currentPos;
        float torqueValue = (distance > 0) ? -0.1f : 0.1f;   // torque values Nm
        PlutoComm.setControlType("TORQUE");
        PlutoComm.setControlTarget(torqueValue);
    }


    private void OnPlutoButtonReleased()
    {
        isCalibrating = true;
    }


    private bool CheckPositionTogether(float currentPosition, float targetPosition)
    {
        if (currentPosition <= 1.5f)
        {
            return true;
        }
        else
        {
            textMessage.text = $"Error: Together Position NOT reached! Current: {currentPosition}";
            textMessage.color = Color.red;
            isCalibrating = false;
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
        }
    }
    private bool CheckPositionTogetherAng(float currentPosition, float targetPosition)
    {
        if (currentPosition <= 1.5f)
        {
            textMessage.text = $"Together Position reached! Current: {currentPosition}";
            return true;
        }
        else
        {
            textMessage.text = $"Error: Together Position NOT reached! Current: {currentPosition}";
            textMessage.color = Color.red;
            isCalibrating = false;
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
        }
    }
    private bool ChkPosTogetherWFEWURD(float currentPosition, float targetPosition)
    {
        if (currentPosition <= 1f)
        {
            textMessage.text = $"Together Position reached! Current: {currentPosition}";
            return true;
        }
        else
        {
            textMessage.text = $"Error: Together Position NOT reached! Current: {currentPosition}";
            textMessage.color = Color.red;
            isCalibrating = false;
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
        }
    }
    private bool ChkPosTogetherFME(float currentPosition, float targetPosition)
    {
        if (currentPosition <= 1f)
        {
            textMessage.text = $"Together Position reached! Current: {currentPosition}";
            return true;
        }
        else
        {
            textMessage.text = $"Error: Together Position NOT reached! Current: {currentPosition}";
            textMessage.color = Color.red;
            isCalibrating = false;
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
        }
    }

    private bool CheckPositionSeparation(float currentPosition, float targetPosition)
    {
        if (currentPosition >= 9.0f)
        {
            return true;
        }
        else
        {
            textMessage.text = $"Error: Separation Position NOT reached! Current: {currentPosition}";
            textMessage.color = Color.red;
            isCalibrating = false;
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
        }
    }
    private bool CheckPositionSeparationAng(float currentPosition, float targetPosition)
    {
        if (currentPosition >= 20.0f)
        {
            textMessage.text = $"Separation Position reached! Current: {currentPosition}";
            return true;
        }
        else
        {
            textMessage.text = $"Error: Separation Position NOT reached! Current: {currentPosition}";
            textMessage.color = Color.red;
            isCalibrating = false;
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
        }
    }
    private bool ChkPosSeparationWFEWURD(float currentPosition, float targetPosition)
    {
        if (currentPosition >= 16.0f)
        {
            textMessage.text = $"Separation Position reached! Current: {currentPosition}";
            return true;
        }
        else
        {
            textMessage.text = $"Error: Separation Position NOT reached! Current: {currentPosition}";
            textMessage.color = Color.red;
            isCalibrating = false;
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
        }
    }
    private bool ChkPosSeparationFME(float currentPosition, float targetPosition)
    {
        if (currentPosition >= 29.0f)
        {
            textMessage.text = $"Separation Position reached! Current: {currentPosition}";
            return true;
        }
        else
        {
            textMessage.text = $"Error: Separation Position NOT reached! Current: {currentPosition}";
            textMessage.color = Color.red;
            isCalibrating = false;
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
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
