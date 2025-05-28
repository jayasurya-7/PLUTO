﻿using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TS.DoubleSlider;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class AssistsceneHandler : MonoBehaviour
{
    enum AssessStates
    {
        INIT,
        ASSESS
    };
    private bool isButtonPressed = false;
    public TMP_Text lText;
    public TMP_Text rText;
    public TMP_Text insText;
    public TMP_Text cText, inst;
    public TMP_Text relaxText;
    
    public TMP_Text jointAngle;
    public TMP_Text jointAngleHoc;
    public TextMeshProUGUI mechName;

    private int _linx, _rinx;
    private float _tmin = 0f, _tmax  =0f;

    public GameObject CurrPositioncursor;
    public GameObject CurrPositioncursorHoc;
    public GameObject redoButton;
    private AssessStates _state;

    private float angLimit;
    public DoubleSlider apromSlider;
    public bool isSelected = false;

    //public assessmentSceneHandler panelControl;

    private List<string[]> DirectionText = new List<string[]>
     {
         new string[] { "Flexion", "Extension" },
         new string[] { "Ulnar Dev", "Radial Dev" },
         new string[] { "Pronation", "Supination" },
         new string[] { "Open", "Open"},
         new string[] { "", "" },
         new string[] { "", "" }
     };


     float currentAngle = PlutoComm.angle;
float targetPositiveEnd ;  // Positive limit
float targetNegativeEnd ; // Negative limit

// Threshold to determine "close enough" to endpoint
float endpointTolerance = 5f;

float torque = 0f;

// Track if reached both ends
bool reachedPositive = false;
bool reachedNegative = false;

// Flags to track which side we're heading toward
bool goingPositive = true;

// For tracking stuck situation
float previousAngle = 0f;
float stuckTimer = 0f;
    float stuckThresholdTime = 2f; // seconds
// Add these as class-level variables:
int positiveStuckAttempts = 0;
int negativeStuckAttempts = 0;
const int maxStuckAttempts = 4;
float maxAngle = 0f;
float minAngle = 0f;
float torqueRampSpeed = 0.1f; // You can tweak this value (e.g., 0.05f, 0.02f for even slower)
bool onceReached = false, firstPositiveStart=true, firstNegativeStart = true;
float trailDuration = 1f;
    float stopClock;
float positiveTimer = 0f;
float negativeTimer = 0f;
    float maxDirectionDuration = 20f;
float fadeOutDuration = 2f;
float fadeOutTimer = 0f;
    bool fadingOut = false;
float positiveMovementTime = 0f;
float negativeMovementTime = 0f;


//torque = Mathf.Min(torque + Time.deltaTime * torqueRampSpeed, 1f); // for positive

    void Start()
    {
          // Set mechanism name
        mechName.text = PlutoComm.MECHANISMSTEXT[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.Instance.selectedMechanism.name)];
  
        InitializeAssessment();
    }
    void ResetAssessment()
{
   // PlutoComm.setControlType("NONE");
    torque = 0f;
    goingPositive = true;
    reachedPositive = false;
    reachedNegative = false;
    onceReached = false;
    firstPositiveStart = true;
    firstNegativeStart = true;
    stuckTimer = 0f;
    positiveStuckAttempts = 0;
    negativeStuckAttempts = 0;
    minAngle = 0f;
    maxAngle = 0f;
    _tmin = 0f;
    _tmax = 0f;
    stopClock = 0f;
    apromSlider.minAng = 0;
    apromSlider.maxAng = 0;
    inst.text = "";
    redoButton.SetActive(false);
}


    public void InitializeAssessment()
    {
        // Disable control.
        ResetAssessment();

        // Update the min and max values.
        angLimit = AppData.Instance.selectedMechanism.IsMechanism("HOC") ? PlutoComm.CALIBANGLE[PlutoComm.mechanism] : PlutoComm.MECHOFFSETVALUE[PlutoComm.mechanism];
        targetNegativeEnd = AppData.Instance.selectedMechanism.IsMechanism("HOC") ? -90f : -angLimit;
        targetPositiveEnd = AppData.Instance.selectedMechanism.IsMechanism("HOC") ? 0f : angLimit;
        apromSlider.Setup(-angLimit, angLimit, -10, 10);
        apromSlider.minAng = 0;
        apromSlider.maxAng = 0;
        // Update central text.
        cText.gameObject.SetActive(AppData.Instance.selectedMechanism.IsMechanism("HOC"));
        cText.text = AppData.Instance.selectedMechanism.IsMechanism("HOC") ? "Closed" : "";

        // Update the left and right text.
        (_rinx, _linx) = AppData.Instance.IsTrainingSide("RIGHT") ? (1, 0) : (0, 1);
        rText.text = DirectionText[PlutoComm.mechanism - 1][_rinx];
        lText.text = DirectionText[PlutoComm.mechanism - 1][_linx];

        // Set the state to INIT.
        _state = AssessStates.INIT;
        inst.text = "";
        // Attach callback for PLUTO button release.
        PlutoComm.OnButtonReleased += OnPlutoButtonReleased;

        UpdateStatusText();
    }



    // void runAssessment()
    // {
    //     stopClock -= Time.deltaTime;
    //     stopClock = Mathf.Max(0, stopClock); // Clamp to 0

    //     float deltaAngle = Mathf.Abs(currentAngle - previousAngle);
    //     bool movingTowardTarget = (goingPositive && !AppData.Instance.selectedMechanism.IsMechanism("HOC")) 
    //                                 ? (currentAngle > previousAngle) 
    //                                 : (currentAngle < previousAngle);

    //     // Stuck detection
    //     if (!movingTowardTarget || deltaAngle < 3f)
    //         stuckTimer += Time.deltaTime;
    //     else
    //         stuckTimer = 0f;

    //     float timeFraction = Mathf.Clamp01((trailDuration - stopClock) / trailDuration); // 0 to 1
    //     float smoothTorque = Mathf.SmoothStep(0f, 1f, timeFraction); // smooth ramp

    //         if (!reachedPositive || !reachedNegative)
    //         {
    //         if (goingPositive)
    //     {
    //         if (!reachedPositive)
    //         {
    //                 if (!reachedPositive && firstPositiveStart)
    //                 {
    //                     trailDuration = 3.5f;
    //                     stopClock = trailDuration;
    //                     torque = 0f;
    //                     onceReached = false;
    //                     firstPositiveStart = false;
    //                 positiveTimer = 0f;

    //             }
    //             positiveTimer += Time.deltaTime;

    //             if (positiveTimer >= maxDirectionDuration)
    //             {
    //                 reachedPositive = true;
    //                 maxAngle = currentAngle;
    //                 torque = 0f;
    //                 PlutoComm.setControlTarget(0);
    //                 goingPositive = false;
    //                 Debug.Log("Timed out after 20s. MaxAngle: " + maxAngle);
    //                 return;
    //             }


    //             if (currentAngle < targetPositiveEnd - endpointTolerance)
    //                 {
    //                     // Set smooth torque ramp
    //                     if (!onceReached)
    //                         torque = smoothTorque;

    //                     // Stuck at max torque
    //                     if (stuckTimer > stuckThresholdTime && torque >= 0.85f)
    //                     {
    //                         stuckTimer = 0f;
    //                         positiveStuckAttempts++;
    //                         onceReached = true; // Mark that we reached or got stuck
    //                     }

    //                     // If stuck too many times, stop
    //                     if (positiveStuckAttempts >= maxStuckAttempts)
    //                     {
    //                         reachedPositive = true;
    //                         maxAngle = currentAngle;
    //                         torque = 0f;
    //                         PlutoComm.setControlTarget(0);
    //                         goingPositive = false;
    //                         Debug.Log("Max torque attempts reached. MaxAngle: " + maxAngle);
    //                         return;
    //                     }

    //                     // Decay torque if direction flips
    //                     if (onceReached && currentAngle > previousAngle)
    //                         torque -= Time.deltaTime * 0.05f;

    //                     PlutoComm.setControlTarget(torque);
    //                 }
    //                 else
    //                 {
    //                     reachedPositive = true;
    //                     maxAngle = currentAngle;
    //                     torque = 0f;
    //                     PlutoComm.setControlTarget(0);
    //                     goingPositive = false;
    //                     Debug.Log("Reached Positive End. MaxAngle: " + maxAngle);
    //                 }
    //         }
    //     }
    //     else
    //     {
    //         if (!reachedNegative)
    //         {

    //                 if (!reachedNegative && firstNegativeStart)
    //                 {
    //                     trailDuration = 3.5f;
    //                     stopClock = trailDuration;
    //                     torque = 0f;
    //                     onceReached = false;
    //                     firstNegativeStart = false;
    //                     negativeTimer = 0f;

    //             }
    //             negativeTimer += Time.deltaTime;

    //             if (negativeTimer >= maxDirectionDuration)
    //             {
    //                 PlutoComm.setControlType("NONE");
    //                 reachedNegative = true;
    //                 minAngle = currentAngle;
    //                 torque = 0f;
    //                 redoButton.SetActive(true);
    //                 Debug.Log("Timed out after 20s. MinAngle: " + minAngle);

    //                 inst.text = $"APROM Reached both ends min : {_tmin},max :{_tmax}. Press PLUTO button to move next scene";
    //                 return;
    //             }

    //             if (currentAngle > targetNegativeEnd + endpointTolerance)
    //                 {
    //                     float revSmoothTorque = -Mathf.SmoothStep(0f, 1f, timeFraction);

    //                     if (!onceReached)
    //                         torque = revSmoothTorque;

    //                     if (stuckTimer > stuckThresholdTime && torque <= -0.85f)
    //                     {
    //                         stuckTimer = 0f;
    //                         negativeStuckAttempts++;
    //                         onceReached = true;
    //                     }

    //                     if (negativeStuckAttempts >= maxStuckAttempts)
    //                     {
    //                         PlutoComm.setControlType("NONE");
    //                         reachedNegative = true;
    //                         minAngle = currentAngle;
    //                         torque = 0f;
    //                         redoButton.SetActive(true);
    //                         Debug.Log("Reached Negative End. MinAngle: " + minAngle);

    //                         inst.text = $"APROM Reached both ends min : {_tmin},max :{_tmax}. Press PLUTO button to move next scene";

    //                         return;
    //                     }

    //                     if (onceReached && currentAngle < previousAngle)
    //                         torque += Time.deltaTime * 0.05f;

    //                     PlutoComm.setControlTarget(torque);
    //                 }
    //                 else
    //                 {
    //                     PlutoComm.setControlType("NONE");
    //                     reachedNegative = true;
    //                     minAngle = currentAngle;
    //                     torque = 0f;
    //                     inst.text = $"APROM Reached both ends min : {_tmin},max :{_tmax}. Press PLUTO button to move next scene";

    //                     redoButton.SetActive(true);
    //                 }
    //         }
    //     }
    //     }
    //     previousAngle = currentAngle;

    //     if (reachedNegative && reachedPositive && isButtonPressed)
    //     {
    //         AppData.Instance.selectedMechanism.SetNewAPromValues(_tmin, _tmax);
    //         PlutoComm.setControlType("NONE");
    //         AppData.Instance.selectedMechanism.SaveAssessmentData();
    //         OnSaveClick();
    //         isButtonPressed = false;
    //         Debug.Log("Assessment complete.");
    //         Debug.Log($"Final Limits => MinAngle: {minAngle}, MaxAngle: {maxAngle}");

    //         if (AppData.Instance.selectedMechanism.apromCompleted)
    //             SceneManager.LoadScene("CHGAME");
    //     }
    // }



    void runAssessment()
{
    stopClock -= Time.deltaTime;
    stopClock = Mathf.Max(0, stopClock); // Clamp to 0

    float deltaAngle = Mathf.Abs(currentAngle - previousAngle);
    bool movingTowardTarget = (goingPositive && !AppData.Instance.selectedMechanism.IsMechanism("HOC")) 
                                ? (currentAngle > previousAngle) 
                                : (currentAngle < previousAngle);

    // Stuck detection
    if (!movingTowardTarget || deltaAngle < 3f)
        stuckTimer += Time.deltaTime;
    else
        stuckTimer = 0f;

    float timeFraction = Mathf.Clamp01((trailDuration - stopClock) / trailDuration);
    float smoothTorque = Mathf.SmoothStep(0f, 1f, timeFraction);

    if (!reachedPositive || !reachedNegative)
    {
        if (goingPositive)
        {
            if (!reachedPositive)
            {
                if (!reachedPositive && firstPositiveStart)
                {
                    trailDuration = 3.5f;
                    stopClock = trailDuration;
                    torque = 0f;
                    onceReached = false;
                    firstPositiveStart = false;
                    positiveTimer = 0f;
                }

                if (movingTowardTarget && deltaAngle >= 3f)
                    positiveMovementTime += Time.deltaTime;

                positiveTimer += Time.deltaTime;

                if (positiveTimer >= maxDirectionDuration)
                {
                    reachedPositive = true;
                    maxAngle = currentAngle;
                    torque = 0f;
                    PlutoComm.setControlTarget(0);
                    goingPositive = false;
                    Debug.Log("Timed out after 20s. MaxAngle: " + maxAngle);
                    Debug.Log("Positive movement time: " + positiveMovementTime);
                    return;
                }

                if (currentAngle < targetPositiveEnd - endpointTolerance)
                {
                    if (!onceReached)
                        torque = smoothTorque;

                    if (stuckTimer > stuckThresholdTime && torque >= 0.85f)
                    {
                        stuckTimer = 0f;
                        positiveStuckAttempts++;
                        onceReached = true;
                    }

                    if (positiveStuckAttempts >= maxStuckAttempts)
                    {
                        reachedPositive = true;
                        maxAngle = currentAngle;
                        torque = 0f;
                        PlutoComm.setControlTarget(0);
                        goingPositive = false;
                        Debug.Log("Max torque attempts reached. MaxAngle: " + maxAngle);
                        Debug.Log("Positive movement time: " + positiveMovementTime);
                        return;
                    }

                    if (onceReached && currentAngle > previousAngle)
                        torque -= Time.deltaTime * 0.05f;

                    PlutoComm.setControlTarget(torque);
                }
                else
                {
                    reachedPositive = true;
                    maxAngle = currentAngle;
                    torque = 0f;
                    PlutoComm.setControlTarget(0);
                    goingPositive = false;
                    Debug.Log("Reached Positive End. MaxAngle: " + maxAngle);
                    Debug.Log("Positive movement time: " + positiveMovementTime);
                }
            }
        }
        else
        {
            if (!reachedNegative)
            {
                if (!reachedNegative && firstNegativeStart)
                {
                    trailDuration = 3.5f;
                    stopClock = trailDuration;
                    torque = 0f;
                    onceReached = false;
                    firstNegativeStart = false;
                    negativeTimer = 0f;
                }

                if (movingTowardTarget && deltaAngle >= 3f)
                    negativeMovementTime += Time.deltaTime;

                negativeTimer += Time.deltaTime;

                if (negativeTimer >= maxDirectionDuration)
                {
                    PlutoComm.setControlType("NONE");
                    reachedNegative = true;
                    minAngle = currentAngle;
                    torque = 0f;
                    redoButton.SetActive(true);
                    Debug.Log("Timed out after 20s. MinAngle: " + minAngle);
                    Debug.Log("Negative movement time: " + negativeMovementTime);

                    inst.text = $"APROM Reached both ends min : {_tmin},max :{_tmax}. Press PLUTO button to move next scene";
                    return;
                }

                if (currentAngle > targetNegativeEnd + endpointTolerance)
                {
                    float revSmoothTorque = -Mathf.SmoothStep(0f, 1f, timeFraction);

                    if (!onceReached)
                        torque = revSmoothTorque;

                    if (stuckTimer > stuckThresholdTime && torque <= -0.85f)
                    {
                        stuckTimer = 0f;
                        negativeStuckAttempts++;
                        onceReached = true;
                    }

                    if (negativeStuckAttempts >= maxStuckAttempts)
                    {
                        PlutoComm.setControlType("NONE");
                        reachedNegative = true;
                        minAngle = currentAngle;
                        torque = 0f;
                        redoButton.SetActive(true);
                        Debug.Log("Reached Negative End. MinAngle: " + minAngle);
                        Debug.Log("Negative movement time: " + negativeMovementTime);

                        inst.text = $"APROM Reached both ends min : {_tmin},max :{_tmax}. Press PLUTO button to move next scene";
                        return;
                    }

                    if (onceReached && currentAngle < previousAngle)
                        torque += Time.deltaTime * 0.05f;

                    PlutoComm.setControlTarget(torque);
                }
                else
                {
                    PlutoComm.setControlType("NONE");
                    reachedNegative = true;
                    minAngle = currentAngle;
                    torque = 0f;
                    redoButton.SetActive(true);
                    Debug.Log("Reached Negative End. MinAngle: " + minAngle);
                    Debug.Log("Negative movement time: " + negativeMovementTime);

                    inst.text = $"APROM Reached both ends min : {_tmin},max :{_tmax}. Press PLUTO button to move next scene";
                }
            }
        }
    }

    previousAngle = currentAngle;

    if (reachedNegative && reachedPositive && isButtonPressed)
    {
        AppData.Instance.selectedMechanism.SetNewAPromValues(_tmin, _tmax);
        PlutoComm.setControlType("NONE");
        AppData.Instance.selectedMechanism.SaveAssessmentData();
        OnSaveClick();
        isButtonPressed = false;
        Debug.Log("Assessment complete.");
        Debug.Log($"Final Limits => MinAngle: {minAngle}, MaxAngle: {maxAngle}");
        Debug.Log($"Positive movement time: {positiveMovementTime}, Negative movement time: {negativeMovementTime}");

        if (AppData.Instance.selectedMechanism.apromCompleted)
            SceneManager.LoadScene("CHGAME");
    }
}

    public void OnExit()
    {
        PlutoComm.setControlType("NONE");
        SceneManager.LoadScene("CHGAME");
    }

    void Update()
    {
        PlutoComm.sendHeartbeat();

        currentAngle = PlutoComm.angle;
        jointAngle.text = $"{((int)PlutoComm.angle).ToString()} + Torque :{PlutoComm.target}";
        jointAngleHoc.text = ((int)PlutoComm.getHOCDisplay(PlutoComm.angle)).ToString();
        runaAssessmentStateMachine();
        // Debug.Log($" ct: {PlutoComm.CONTROLTYPE[PlutoComm.controlType]} + tor :{PlutoComm.target}");
    }

    void runaAssessmentStateMachine()
    {
        Debug.Log($"state : {_state}");
        CurrPositioncursor.SetActive(true);
        CurrPositioncursorHoc.SetActive(AppData.Instance.selectedMechanism.IsMechanism("HOC"));
        switch (_state)
        {
            case AssessStates.INIT:
                if (isButtonPressed || Input.GetKeyDown(KeyCode.Return))
                {
                        PlutoComm.setControlType("TORQUE");

                    //if(PlutoComm.CONTROLTYPE[PlutoComm.controlType]=="TORQUE") startAssessment();
                    startAssessment();
                    isButtonPressed = false;
                }
               // relaxText.text = FormatRelaxText(AppData.Instance.selectedMechanism.oldRom.promMin, AppData.Instance.selectedMechanism.oldRom.promMax);
                break;
            case AssessStates.ASSESS:
                runAssessment();
                _tmin = apromSlider.minAng;
                _tmax = apromSlider.maxAng;
                break;
        }
    }

    public void OnRedoPromClick()
    {
        InitializeAssessment();
        Debug.Log("Redo PROM: Reset to INIT state.");
    }

    public void OnPlutoButtonReleased()
    {
        isButtonPressed = true;
    }

    private float ConvertToCM(float value) => Mathf.Abs(Mathf.Deg2Rad * value * 6f);

    public void OnNextButtonClick()
    {
        PlutoComm.setControlType("NONE");
        OnSaveClick();

    }

    public void OnSaveClick()
    {
        // Update new PROM
       // AppData.Instance.selectedMechanism.SetNewPromValues(apromSlider.minAng, apromSlider.maxAng);
        Debug.Log($"min: {apromSlider.minAng}, max :{apromSlider.maxAng}");
        apromSlider.UpdateMinMaxvalues = false;
        CurrPositioncursor.SetActive(false);
        CurrPositioncursorHoc.SetActive(false);
    }

    private string FormatRelaxText(float min, float max)
    {
        return AppData.Instance.selectedMechanism.IsMechanism("HOC") ?
            $"Prev PROM: {ConvertToCM(min).ToString("0.0")}cm : {ConvertToCM(max).ToString("0.0")}cm (Aperture: {ConvertToCM(max - min).ToString("0.0")}cm)" :
            $"Prev PROM: {(int)min} : {(int)max} ({(int)(max - min)}°)";
    }

    public void startAssessment()
    {
        _state = AssessStates.ASSESS;
        apromSlider.minAng = 0;
        apromSlider.maxAng = 0;
        Debug.Log("Assessment started");
        apromSlider.startAssessment(PlutoComm.angle);
        apromSlider.UpdateMinMaxvalues = true;
    }

    private void UpdateStatusText()
    {
        if (AppData.Instance.selectedMechanism.IsMechanism("HOC") == false)
        {
            jointAngle.text = $"{(PlutoComm.angle).ToString("0.0")}+ torque :{PlutoComm.target}";
        }
        else
        {
            jointAngle.text = "Aperture" + ConvertToCM(PlutoComm.angle).ToString("0.0") + "cm";
            jointAngleHoc.text = "Aperture" + ConvertToCM(PlutoComm.angle).ToString("0.0") + "cm";
        }
    }
}