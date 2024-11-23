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
    private float togetherPositionz = 1.5f;
    private float togetherAnglez = 1f;
    private float dist;
    private float separationPosition = 11.0f;
    private float separationAngle = 180.0f;
    private float separationAngleWFE = 140.0f;
    private float separationPositionz = 8.0f;
    private float separationAnglez= 170.0f;
    private float separationAngleWFEz = 135.0f;
    public TextMeshProUGUI textMessage;
    public TextMeshProUGUI mechText;
    public TextMeshProUGUI angle;

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
        angle.text = PlutoComm.angle.ToString("f2");
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
        Debug.Log("Current Distance: "+ currentDistance);
        ApplyTorqueToMoveHandleszxx(currentDistance, togetherPosition);
        yield return new WaitForSeconds(1f);
        

        PlutoComm.calibrate(selectedMechanism);

        ApplyTorqueToMoveHandlesz(PlutoComm.getHOCDisplay(PlutoComm.angle), separationPosition);

        yield return new WaitForSeconds(1f);

        if (!CheckPositionSeparation(PlutoComm.getHOCDisplay(PlutoComm.angle), separationPositionz)) yield break;

        ApplyTorqueToMoveHandleszxx(PlutoComm.getHOCDisplay(PlutoComm.angle), togetherPosition);

        yield return new WaitForSeconds(1.0f);
        if (!CheckPositionTogether(PlutoComm.getHOCDisplay(PlutoComm.angle), togetherPositionz)) yield break;
        isCalibrating = false;
        textMessage.text = "Calibration Done";
        textMessage.color = new Color32(62, 214, 111, 255);
        PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);

        Invoke("LoadNextScene", 0.4f);
    }

    IEnumerator autoCalibrateFPS()
    {
        //PlutoComm.calibrate(AppData.selectedMechanism);
        Debug.Log("selected Mech"+ AppData.selectedMechanism);
        textMessage.color = Color.black;
        textMessage.text = "Calibrating...";

        float currentAngle =PlutoComm.angle;

        ApplyTorqueToMoveHandleszx(currentAngle, togetherAngle);
        yield return new WaitForSeconds(1f);
        
        PlutoComm.calibrate(AppData.selectedMechanism);

        ApplyTorqueToMoveHandlesz(PlutoComm.angle, separationAngle);

        yield return new WaitForSeconds(1f);
        currentAngle = PlutoComm.angle;
        if (!CheckPositionSeparation(PlutoComm.angle, separationAnglez)) yield break;
        ApplyTorqueToMoveHandlesz(currentAngle, togetherAngle);


        yield return new WaitForSeconds(1.0f);
        currentAngle = PlutoComm.angle;

        if (!CheckPositionTogether(PlutoComm.angle, togetherAnglez)) yield break;
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

        ApplyTorqueToMoveHandleszx(PlutoComm.angle, togetherAngle);

        yield return new WaitForSeconds(1.0f);
        PlutoComm.calibrate(selectedMechanism);

        ApplyTorqueToMoveHandlesz(PlutoComm.angle, separationAngleWFE);

        yield return new WaitForSeconds(1.0f);
        if (!CheckPositionSeparation(PlutoComm.angle, separationAnglez)) yield break;


        ApplyTorqueToMoveHandlesz(PlutoComm.angle, togetherAngle);
        yield return new WaitForSeconds(1.0f);
        if (!CheckPositionTogether(PlutoComm.angle, togetherAnglez)) yield break;

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

        ApplyTorqueToMoveHandleszx(PlutoComm.angle, togetherAngle);

        yield return new WaitForSeconds(1.0f); 

        PlutoComm.calibrate(selectedMechanism);

        ApplyTorqueToMoveHandlesz(PlutoComm.angle, separationAngle);

        yield return new WaitForSeconds(1.0f);
        if (!CheckPositionSeparation(PlutoComm.angle, separationAnglez)) yield break;


        ApplyTorqueToMoveHandlesz(PlutoComm.angle, togetherAngle);


        yield return new WaitForSeconds(1.0f);
        if (!CheckPositionTogether(PlutoComm.angle, togetherAnglez)) yield break;

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
    private void ApplyTorqueToMoveHandlesz(float currentPos, float targetPos)
    {
        float torqueValue = (currentPos > 0) ? -0.1f : 0.1f;   // torque values Nm

        PlutoComm.setControlType("TORQUE");
        PlutoComm.setControlTarget(torqueValue);

    }
    private void ApplyTorqueToMoveHandleszx(float currentPos, float targetPos)
    {
        //float distance = targetPos - currentPos;
        float torqueValue = (currentPos > 0) ? -0.1f : -0.1f;   // torque values Nm
        Debug.Log("Distance: " + currentPos + " , " + "torqueValue :" + torqueValue);

        PlutoComm.setControlType("TORQUE");
        PlutoComm.setControlTarget(torqueValue);

    }
    private void ApplyTorqueToMoveHandleszxx(float currentPos, float targetPos)
    {
        //float distance = targetPos - currentPos;
        float torqueValue = (currentPos > 0) ? 0.1f : 0.1f;   // torque values Nm
        Debug.Log("Distance: " + currentPos + " , " + "torqueValue :" + torqueValue);

        PlutoComm.setControlType("TORQUE");
        PlutoComm.setControlTarget(torqueValue);

    }

    private void OnPlutoButtonReleased()
    {
        isCalibrating = true;
    }


    private bool CheckPositionTogether(float currentPosition, float targetPosition)
    {
        if (currentPosition <= targetPosition)
        {
            return true;
        }
        else
        {
            textMessage.text = $"Try Again.{currentPosition}";
            textMessage.color = Color.red;
            isCalibrating = false;
            PlutoComm.calibrate(AppData.selectedMechanism);
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
        }
    }

    private bool CheckPositionSeparation(float currentPosition, float targetPosition)
    {
        if (currentPosition >= targetPosition)
        {
            textMessage.text = $"Separation Position reached! Current: {currentPosition} and {targetPosition}";
            return true;
        }
        else
        {
            textMessage.text = $"Error: Separation Position NOT reached! Current: {currentPosition}";
            textMessage.color = Color.red;
            PlutoComm.calibrate("NOMECH");
            isCalibrating = false;
            PlutoComm.setControlType(PlutoComm.CONTROLTYPE[0]);
            return false;
        }
    }
   
    
    
    private void OnExitButtonClicked()
    {
        PlutoComm.calibrate(PlutoComm.MECHANISMS[0]);
        Debug.Log("PlutoComm.MECHANISMS[0]:" + PlutoComm.MECHANISMS[0]);
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
