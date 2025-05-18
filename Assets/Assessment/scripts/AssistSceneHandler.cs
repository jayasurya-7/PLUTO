using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TS.DoubleSlider;
using UnityEngine.SceneManagement;


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
    public TMP_Text cText;
    public TMP_Text relaxText;

    public TMP_Text jointAngle;
    public TMP_Text jointAngleHoc;

    private int _linx, _rinx;
    private float _tmin = 0f, _tmax  =0f;

    public GameObject CurrPositioncursor;
    public GameObject CurrPositioncursorHoc;

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
float endpointTolerance = 2f;

float torque = 0f;

// Track if reached both ends
bool reachedPositive = false;
bool reachedNegative = false;

// Flags to track which side we're heading toward
bool goingPositive = true;

// For tracking stuck situation
float previousAngle = 0f;
float stuckTimer = 0f;
    float stuckThresholdTime = 1.5f; // seconds
// Add these as class-level variables:
int positiveStuckAttempts = 0;
int negativeStuckAttempts = 0;
const int maxStuckAttempts = 4;
float maxAngle = 0f;
float minAngle = 0f;
float torqueRampSpeed = 0.1f; // You can tweak this value (e.g., 0.05f, 0.02f for even slower)

//torque = Mathf.Min(torque + Time.deltaTime * torqueRampSpeed, 1f); // for positive

    void Start()
    {
        InitializeAssessment();
    }

    public void InitializeAssessment()
    {
        // Disable control.
    

        // Update the min and max values.
            angLimit = AppData.Instance.selectedMechanism.IsMechanism("HOC") ? PlutoComm.CALIBANGLE[PlutoComm.mechanism] : PlutoComm.MECHOFFSETVALUE[PlutoComm.mechanism];
        targetNegativeEnd =AppData.Instance.selectedMechanism.IsMechanism("HOC")? -90f: -angLimit;
        targetPositiveEnd = AppData.Instance.selectedMechanism.IsMechanism("HOC")? 0f: angLimit;
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
        
        // Attach callback for PLUTO button release.
        PlutoComm.OnButtonReleased += OnPlutoButtonReleased;

        UpdateStatusText();
    }
//     void runAssessment()
// {
//     float deltaAngle = Mathf.Abs(currentAngle - previousAngle);
//     bool movingTowardTarget = (goingPositive && !AppData.Instance.selectedMechanism.IsMechanism("HOC") )? (currentAngle > previousAngle) : (currentAngle < previousAngle);

//     if (!movingTowardTarget || deltaAngle < 3f)
//         stuckTimer += Time.deltaTime;
//     else
//         stuckTimer = 0f;

//     if (goingPositive) // && !AppData.Instance.selectedMechanism.IsMechanism("HOC"))
//     {
//         if (!reachedPositive)
//         {
//             if (currentAngle < targetPositiveEnd - endpointTolerance)
//             {
//                 // Always gradually increase torque to max
//                 torque = Mathf.Min(torque + Time.deltaTime, 1f);

//                 // If stuck too long at max torque, give up
//                 if (stuckTimer > stuckThresholdTime && torque >= 1f)
//                     positiveStuckAttempts++;

//                 if (positiveStuckAttempts >= maxStuckAttempts)
//                 {
//                     reachedPositive = true;
//                     maxAngle = currentAngle;
//                     Debug.Log("Max torque limit reached. Saving maxAngle: " + maxAngle);
//                     torque = 0f;
//                     PlutoComm.setControlTarget(0);
//                     goingPositive = false;
//                     return;
//                 }

//                 PlutoComm.setControlTarget(torque);
//             }
//             else
//             {
//                 reachedPositive = true;
//                 maxAngle = currentAngle;
//                 torque = 0f;
//                 PlutoComm.setControlTarget(0);
//                 goingPositive = false;
//                 Debug.Log("Reached targetPositiveEnd. Saving maxAngle: " + maxAngle);
//             }
//         }
//     }
//     else
//     {
//         if (!reachedNegative)
//         {
//             if (currentAngle > targetNegativeEnd + endpointTolerance)
//             {
//                 // Gradually increase reverse torque
//                 torque = Mathf.Max(torque - Time.deltaTime, -1f);

//                 // If stuck too long at max reverse torque, give up
//                 if (stuckTimer > stuckThresholdTime && torque <= -1f)
//                     negativeStuckAttempts++;

//                 if (negativeStuckAttempts >= maxStuckAttempts)
//                 {
//                     reachedNegative = true;
//                     minAngle = currentAngle;
//                     Debug.Log("Min torque limit reached. Saving minAngle: " + minAngle);
//                     torque = 0f;
//                     PlutoComm.setControlType("NONE");
//                     PlutoComm.setControlTarget(0);
//                     return;
//                 }

//                 PlutoComm.setControlTarget(torque);
//             }
//             else
//             {
//                 reachedNegative = true;
//                 minAngle = currentAngle;
//                 torque = 0f;
//                 PlutoComm.setControlTarget(0);
//                 Debug.Log("Reached targetNegativeEnd. Saving minAngle: " + minAngle);
//             }
//         }
//     }

//     previousAngle = currentAngle;

//     if (reachedPositive && reachedNegative && isButtonPressed)
//     {
//         PlutoComm.setControlTarget(0);
//         PlutoComm.setControlType("NONE");
//         isButtonPressed = false;
//         Debug.Log("Reached both ends. Stopping control.");
//         Debug.Log($"Final Limits => MinAngle: {minAngle}, MaxAngle: {maxAngle}");
//     }
// }

//--------------------------------------------------------------------------------------------

// void runAssessment()
//     {
//         float deltaAngle = Mathf.Abs(currentAngle - previousAngle);
//         bool movingTowardTarget = (goingPositive && !AppData.Instance.selectedMechanism.IsMechanism("HOC") ) ? (currentAngle > previousAngle) : (currentAngle < previousAngle);

//         if (!movingTowardTarget || deltaAngle < 4f)
//             stuckTimer += Time.deltaTime;
//         else
//             stuckTimer = 0f;

//         if (goingPositive)
//         {
//             if (!reachedPositive)
//             {
//                 if (currentAngle < targetPositiveEnd - endpointTolerance)
//                 {
//                     if (stuckTimer > stuckThresholdTime)
//                         torque = Mathf.Min(torque + Time.deltaTime, 1f);
//                     else
//                         torque = 0.3f;

//                     PlutoComm.setControlTarget(torque);
//                 }
//                 else
//                 {
//                     reachedPositive = true;
//                     goingPositive = false;
//                     torque = 0f;
//                     PlutoComm.setControlTarget(0);
//                 }
//             }
//         }
//         else
//         {
//             if (!reachedNegative)
//             {
//                 if (currentAngle > targetNegativeEnd + endpointTolerance)
//                 {
//                     if (stuckTimer > stuckThresholdTime)
//                         torque = Mathf.Max(torque - Time.deltaTime, -1f);
//                     else
//                         torque = -0.3f;

//                     PlutoComm.setControlTarget(torque);
//                 }
//                 else
//                 {
//                     reachedNegative = true;
//                     torque = 0f;
//                     PlutoComm.setControlTarget(0);
//                 }
//             }
//         }

//         previousAngle = currentAngle;

//         if (reachedPositive && reachedNegative && isButtonPressed)
//         {
//             PlutoComm.setControlTarget(0);
//             PlutoComm.setControlType("NONE");
//             isButtonPressed = false;
//             Debug.Log("Reached both ends. Stopping control.");
//         }
//     }




    void runAssessment()
    {
        float deltaAngle = Mathf.Abs(currentAngle - previousAngle);
        bool movingTowardTarget = (goingPositive && !AppData.Instance.selectedMechanism.IsMechanism("HOC") )?(currentAngle > previousAngle) : (currentAngle < previousAngle);

        if (!movingTowardTarget || deltaAngle < 3f)
            stuckTimer += Time.deltaTime;
        else
            stuckTimer = 0f;

        if (goingPositive)
        {
            if (!reachedPositive)
            {
                if (currentAngle < targetPositiveEnd - endpointTolerance)
                {
                    // if (stuckTimer > stuckThresholdTime)
                    // {
                    //     torque = Mathf.Min(torque + Time.deltaTime, 1f);
                    //     if (torque >= 1f)
                    //         positiveStuckAttempts++;

                    //     if (positiveStuckAttempts >= maxStuckAttempts)
                    //     {
                    //         reachedPositive = true;
                    //         maxAngle = currentAngle;
                    //         Debug.Log("Max torque limit reached. Saving maxAngle: " + maxAngle);
                    //         torque = 0f;
                    //         PlutoComm.setControlTarget(0);
                    //         goingPositive = false;
                    //         return;
                    //     }
                    // }
                    // else
                    // {
                    //     torque = 0.1f;
                    // }

                    // PlutoComm.setControlTarget(torque);

                    if (stuckTimer > stuckThresholdTime)
                    {
                        torque = Mathf.Min(torque + Time.deltaTime * torqueRampSpeed, 1f);
                        if (torque >= 1f)
                            positiveStuckAttempts++;

                        if (positiveStuckAttempts >= maxStuckAttempts)
                        {
                            reachedPositive = true;
                            maxAngle = currentAngle;
                            Debug.Log("Max torque limit reached. Saving maxAngle: " + maxAngle);
                            torque = 0f;
                            PlutoComm.setControlTarget(0);
                            goingPositive = false;
                            return;
                        }
                    }
                    else
                    {
                        torque = 0.1f;
                    }

                    PlutoComm.setControlTarget(torque);

                    
                }
                else
                {
                    reachedPositive = true;
                    maxAngle = currentAngle;
                    torque = 0f;
                    PlutoComm.setControlTarget(0);
                    goingPositive = false;
                    Debug.Log("Reached targetPositiveEnd. Saving maxAngle: " + maxAngle);
                }
            }
        }
        else
        {
            if (!reachedNegative)
            {
                if (currentAngle > targetNegativeEnd + endpointTolerance)
                {
                    if (stuckTimer > stuckThresholdTime)
                    {
                        // torque = Mathf.Max(torque - Time.deltaTime, -1f);
                        torque = Mathf.Min(torque + Time.deltaTime * torqueRampSpeed, 1f);
                        if (torque <= -1f)
                            negativeStuckAttempts++;

                        if (negativeStuckAttempts >= maxStuckAttempts)
                        {
                            reachedNegative = true;
                            minAngle = currentAngle;
                            Debug.Log("Min torque limit reached. Saving minAngle: " + minAngle);
                            torque = 0f;
                            PlutoComm.setControlType("NONE");
                            PlutoComm.setControlTarget(0);
                            return;
                        }
                    }
                    else
                    {
                        // Smooth negative torque increase instead of abrupt jump
                torque = Mathf.Max(torque - Time.deltaTime, -1f);
                    }

                    PlutoComm.setControlTarget(torque);
                }
                else
                {
                    reachedNegative = true;
                    minAngle = currentAngle;
                    torque = 0f;
                    PlutoComm.setControlTarget(0);
                    Debug.Log("Reached targetNegativeEnd. Saving minAngle: " + minAngle);
                }
            }
        }

        previousAngle = currentAngle;

        if (reachedPositive && reachedNegative && isButtonPressed)
        {
            PlutoComm.setControlTarget(0);
            PlutoComm.setControlType("NONE");
            isButtonPressed = false;
            Debug.Log("Reached both ends. Stopping control.");
            Debug.Log($"Final Limits => MinAngle: {minAngle}, MaxAngle: {maxAngle}");
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
        Debug.Log($" ct: {PlutoComm.CONTROLTYPE[PlutoComm.controlType]} + tor :{PlutoComm.target}");
    }

    void runaAssessmentStateMachine()
    {
        CurrPositioncursor.SetActive(true);
        CurrPositioncursorHoc.SetActive(AppData.Instance.selectedMechanism.IsMechanism("HOC"));
        switch (_state)
        {
            case AssessStates.INIT:
                if (isButtonPressed || Input.GetKeyDown(KeyCode.Return))
                {
                    // while (PlutoComm.CONTROLTYPE[PlutoComm.controlType] != "TORQUE")
                    // {
                        PlutoComm.setControlType("TORQUE");
                    // }  

                   if(PlutoComm.CONTROLTYPE[PlutoComm.controlType]=="TORQUE") startAssessment();
                    isButtonPressed = false;
                }
               // relaxText.text = FormatRelaxText(AppData.Instance.selectedMechanism.oldRom.promMin, AppData.Instance.selectedMechanism.oldRom.promMax);
                break;
            case AssessStates.ASSESS:
                runAssessment();
                _tmin = apromSlider.minAng;
                _tmax = apromSlider.maxAng;
               // relaxText.text = FormatRelaxText(AppData.Instance.selectedMechanism.oldRom.promMin, AppData.Instance.selectedMechanism.oldRom.promMax);
                
                if (isButtonPressed || Input.GetKeyDown(KeyCode.Return))
                {
                    OnNextButtonClick();
                    isButtonPressed = false;
                }
                break;
        }
    }

    public void OnRedoPromClick()
    {
        _state = AssessStates.INIT;
        isButtonPressed = false;

        // Reinitialize the assessment process
        InitializeAssessment();

        UpdateStatusText();
      //  AppData.Instance.selectedMechanism.ResetAromValues();
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
    public void OnrestartButtonClick()
    {
        Start();
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




