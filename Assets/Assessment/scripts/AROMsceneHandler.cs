// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Text;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;
// using TMPro;
// using TS.DoubleSlider;
// using UnityEngine.UIElements;
// using System.IO;


// public class AROMsceneHandler : MonoBehaviour
// {
//     enum AssessStates
//     {
//         INIT = 0,
//         ASSESS = 1
//     };
//     bool assessmentSaved;
//     public TMP_Text lText;
//     public TMP_Text rText;
//     public TMP_Text cText;
//     public TMP_Text relaxText;

//     public TMP_Text JointAngle;
//     public TMP_Text JointAngleHoc;

//     public TMP_Text warningText;
//     bool AssessmentValid;
//     private float _tmin, _tmax, _tmin1, _tmax1;
//     private float prommin, prommax;
   
//     public GameObject nextButton;
//     public GameObject startButton;
//     public GameObject curreposition;
//     public GameObject currepositionHoc;
//     private AssessStates _state;
//     private float angLimit;
//     public DoubleSlider aromSlider;
//     public DoubleSlider promSlider;

//     public bool isSelected = false;
//     public bool isInteractable = false;
//     public assessmentSceneHandler panelControl;

//     private bool isRestarting = false;
//     private bool isButtonPressed = false;

//     private List<string[]> DirectionText = new List<string[]>
//     {
//         new string[] { "Flexion", "Extension" },
//         new string[] { "Ulnar Dev.", "Radial Dev."},
//         new string[] { "Pronation", "Supination" },
//         new string[]{ "Open", "Open"},
//         new string[] {"",""},
//         new string[] {"",""}
//     };

//     private int _linx, _rinx;

//     void Start()
//     {

//         //InitializeAssessment();
//         //aromSlider.UpdateMinMaxvalues = false;
//     }

//     private void InitializeAssessment()
//     {
//         aromSlider.UpdateMinMaxvalues = false;
//         gameData.isAROMcompleted = false;
//         nextButton.SetActive(false);
//         Debug.Log("Initializing AROM assessment");

//         string dir = Path.Combine(DataManager.directoryAPROMData, AppData.selectedMechanism + ".csv");
//         if (!Directory.Exists(DataManager.directoryAPROMData))
//         {
//             Directory.CreateDirectory(DataManager.directoryAPROMData);
//         }
//         if (!File.Exists(dir))
//         {
//             using (var writer = new StreamWriter(dir, false, Encoding.UTF8))
//             {
//                 writer.WriteLine("datetime,promTmin,promTmax,aromTmin,aromTmax");
//             }
//         }
//         if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) != 4)
//         {
//             angLimit = AppData.offsetAtNeutral[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)];

//             aromSlider.Setup(-angLimit, angLimit, AppData.oldAROM.aromTmin, AppData.oldAROM.aromTmax);
//             aromSlider.maxAng = 0;
//             aromSlider.minAng = 0;
//             aromSlider.UpdateMinMaxvalues = false;
//             Debug.Log($"Slider Min: {aromSlider.minAng}, Max: {aromSlider.maxAng}, arom:{AppData.oldAROM.aromTmin},{AppData.oldAROM.aromTmax}");

//         }
//         else
//         {

//             angLimit = 100.42f;

//             aromSlider.Setup(-angLimit, angLimit, AppData.oldAROM.aromTmin, AppData.oldAROM.aromTmax);


//             aromSlider.minAng = 0;  // Set slider minimum to old AROM minimum
//             aromSlider.maxAng = 0;
//             aromSlider.UpdateMinMaxvalues = false;


//         }
//         if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 4)
//         {
//             cText.gameObject.SetActive(true); // Show the C Text
//             rText.gameObject.SetActive(true);

//             lText.gameObject.SetActive(true);

//             cText.text = "Closed"; // Set the C Text in the center
//         }
//         else
//         {
//             cText.gameObject.SetActive(false);
//         }
//         if (AppData.trainingSide == "right")
//         {
//             _rinx = 1;
//             _linx = 0;
//         }
//         else
//         {
//             _rinx = 0;
//             _linx = 1;
//         }
//         rText.gameObject.SetActive(true);
//         lText.gameObject.SetActive(true);
//         rText.text = DirectionText[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)][_rinx];
//         lText.text = DirectionText[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)][_linx];

//         _tmin = 180f;
//         _tmax = -180f;
//         _tmin1 = 180f;
//         _tmax1 = -180f;



//         _state = AssessStates.INIT;

//         UpdateGUI();
//         PlutoComm.setControlType("NONE");
//     }

//     public void OnStartButtonClick()
//     {
//         startAssessment(); 
//         startButton.SetActive(false);
//         nextButton.SetActive(true);
//     }

//     private void RestartAssessment()
//     {
//         InitializeAssessment();
//     }
//     public void OnPlutoButtonReleased()
//     {

//         isButtonPressed = true;

//     }
//     void Update()
//     {

//         if (isSelected)
//         {
//             switch (_state)
//             {
//                 case AssessStates.INIT:
//                     gameData.isAROMcompleted = false;
//                     if (!isInteractable)
//                     {

//                         AppData.oldAROM = new ROM(AppData.selectedMechanism);
//                         PlutoComm.OnButtonReleased += OnPlutoButtonReleased;
//                         InitializeAssessment();
//                         isInteractable = true;
//                     }
//                     startButton.SetActive(true);


//                     prommin = AppData.promTmin;

//                     prommax = AppData.promTmax;

//                     if (isButtonPressed || Input.GetKeyDown(KeyCode.Return))
//                     {
//                         startAssessment();
//                         isButtonPressed = false;
//                     }
//                     if (isRestarting)
//                     {
//                         relaxText.color = Color.red;
//                         relaxText.text = " AROM Should not Exceed PROM \n " +
//                                          "Please REDO PROM AGAIN";
//                     }
//                     else
//                         if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 4)
//                     {
//                         float apertureMinCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldAROM.aromTmin * 6f);
//                         float apertureMaxCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldAROM.aromTmax * 6f);
//                         //relaxText.color = Color.black;
//                         relaxText.text = "Prev AROM: " + apertureMinCM.ToString("0.0") + "cm : " + apertureMaxCM.ToString("0.0") + 
//                             "cm (Aperture: " + Mathf.Abs(apertureMaxCM - apertureMinCM).ToString("0.0") + "cm)";

//                     }
//                     else
//                     {
//                         relaxText.text = "Prev AROM: " + (int)AppData.oldAROM.aromTmin + " : " + (int)AppData.oldAROM.aromTmax +
//                             " (" + (int)(AppData.oldAROM.aromTmax - AppData.oldAROM.aromTmin) + "°)";
                       
//                     }
//                     break;
//                 case AssessStates.ASSESS:
                
//                     _tmin = aromSlider.minAng;
//                     _tmax =aromSlider.maxAng;
//                       if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) != 4)
//                         {
//                         relaxText.color = Color.white;
//                             relaxText.text = "Prev AROM: " + (int)AppData.oldAROM.aromTmin + " : " + (int)AppData.oldAROM.aromTmax +
//                                 " (" + (int)(AppData.oldAROM.aromTmax - AppData.oldAROM.aromTmin) + "°)" ;
//                         }
//                         else
//                         {
//                             float apertureMinCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldAROM.aromTmin * 6f);
//                             float apertureMaxCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldAROM.aromTmax * 6f);
//                             relaxText.color = Color.white;
//                             relaxText.text = "Prev Prom: " + apertureMinCM.ToString("0.0") + "cm : " +
//                                 apertureMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(apertureMaxCM - apertureMinCM).ToString("0.0") + "cm)";

//                         }
//                         nextButton.SetActive(true);

//                         gameData.isAROMcompleted = true;

//                         if (isButtonPressed || Input.GetKeyDown(KeyCode.Return))
//                         {
//                             OnNextButtonClick();
//                             isButtonPressed = false;
//                         }
                    
//                     aromgreater();

//                     break;
//             }
//             UpdateGUI();
//         }
      
//     }

//     private void aromgreater()
//     {
//         if (aromSlider._currePostion.value <= prommin || aromSlider._currePostion.value >= prommax)
//         {

//             aromSlider.UpdateMinMaxvalues = false;
//             RestartAssessment();
//             isRestarting = true;
//             gameData.isAROMcompleted = false;
//             curreposition.SetActive(true);
//             if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 4)
//             {
//                 currepositionHoc.SetActive(true);
//             }
//             else
//             {
//                 currepositionHoc.SetActive(false);
//             }
//             relaxText.text = " AROM Do not Exceed PROM \n " +
//             "Please REDO PROM AGAIN";
//         }
//         else
//         {
//             aromSlider.UpdateMinMaxvalues = true;
//             curreposition.SetActive(true);
//             if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 4)
//             {
//                 currepositionHoc.SetActive(true);
//             }
//             else
//             {
//                 currepositionHoc.SetActive(false);
//             }
//             //relaxText.text = " AROM Do not Exceed PROM \n " +
//             //"Please REDO PROM AGAIN"; ;
//         }
//     }

//     public void OnRedoaromButtonClick()
//     {

//         InitializeAssessment();
//         Debug.Log("Assessment Restarted");
//         Start();
//         aromSlider.UpdateMinMaxvalues = false;
//     }

//     public void aromButton()
//     {
//         Start();
//     }
//     public void OnNextButtonClick()
//     {
//         aromgreater();
//         onSavePressed();
//         nextButton.SetActive(false);
//         aromSlider.UpdateMinMaxvalues = false;
//     }

//     public void startAssessment()
//     {
//         _state = AssessStates.ASSESS;
//         nextButton.SetActive(false);
//         startButton.SetActive(false);
//         aromSlider.startAssessment(PlutoComm.angle);
//         aromSlider.UpdateMinMaxvalues = true;
//     }

 
//     public void onSavePressed()
//     {
//         _tmin = aromSlider.minAng;
//         _tmax = aromSlider.maxAng;

//         AppData.newAROM = new ROM(AppData.promTmin,AppData.promTmax,_tmin, _tmax,
//          AppData.selectedMechanism, true);

//         if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 4)
//         {
//             float apertureMinCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldAROM.aromTmin * 6f);
//             float apertureMaxCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldAROM.aromTmax * 6f);
//             float currentMinCM = Mathf.Abs(Mathf.Deg2Rad * _tmin * 6f);
//             float currentMaxCM = Mathf.Abs(Mathf.Deg2Rad * -_tmax * 6f);
//             relaxText.color = Color.white;
//             relaxText.text = "Assessment Completed \n" + "Prev AROM: " + apertureMinCM.ToString("0.0") + "cm : " +
//                 apertureMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(apertureMaxCM - apertureMinCM).ToString("0.0") + "cm)\n" +
//                     "Current AROM: " + currentMinCM.ToString("0.0") + "cm : " + currentMaxCM.ToString("0.0") + "cm (Aperture: " + 
//                     Mathf.Abs(currentMaxCM - currentMinCM).ToString("0.0") + "cm)\n";
//         }
//         else
//         {
//             relaxText.color = Color.white;
//             relaxText.text = "Assessment Completed \n " + "Prev AROM: " + (int)AppData.oldAROM.aromTmin + " : " + 
//                 (int)AppData.oldAROM.aromTmax + " (" + (int)(AppData.oldAROM.aromTmax - AppData.oldAROM.aromTmin) + "°)\n" +
//             "Current AROM: " + (int)_tmin + " : " + (int)_tmax + " (" + (int)(_tmax - _tmin) + "°)\n";
//         }

//         nextButton.SetActive(false);
//         aromSlider.UpdateMinMaxvalues = false;

//         if (gameData.isPROMcompleted && gameData.isAROMcompleted)
//         {
//             gameData.setNeutral = true;
//             SceneManager.LoadScene("choosegame");
//         }
//     }


 



//     private void UpdateGUI()
//     {
//         UpdateStatusText();
//     }

//     private void UpdateStatusText()
//     {
//         if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) != 4)
//         {
//             JointAngle.text = (PlutoComm.angle).ToString("0.0");

//         }
//         else
//         {

//             JointAngle.text = "Aperture" + Mathf.Abs((Mathf.Deg2Rad * PlutoComm.angle * 6f)).ToString("0.0") + "cm";
//             JointAngleHoc.text = "Aperture" + Mathf.Abs((Mathf.Deg2Rad * PlutoComm.angle * 6f)).ToString("0.0") + "cm";

//         }
//     }

// }



using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using TS.DoubleSlider;

public class AROMsceneHandler : MonoBehaviour
{
    private enum AssessStates { INIT, ASSESS };
    
    private AssessStates _state;
    private bool assessmentSaved;
    private bool isRestarting;
    private bool isButtonPressed;
    private bool AssessmentValid;
    private float _tmin, _tmax;
    private float prommin, prommax;
    private int _linx, _rinx;
    private float angLimit;

    public TMP_Text lText, rText, cText, relaxText, JointAngle, JointAngleHoc, warningText;
    public GameObject nextButton, startButton, curreposition, currepositionHoc;
    public DoubleSlider aromSlider, promSlider;
    public assessmentSceneHandler panelControl;
    public bool isSelected = false, isInteractable = false;

    private static readonly string[][] DirectionText = {
        new string[] { "Flexion", "Extension" },
        new string[] { "Ulnar Dev.", "Radial Dev."},
        new string[] { "Pronation", "Supination" },
        new string[] { "Open", "Open" },
        new string[] { "", "" },
        new string[] { "", "" }
    };

    void Start() => InitializeAssessment();

    private void InitializeAssessment()
    {
        aromSlider.UpdateMinMaxvalues = false;
        gameData.isAROMcompleted = false;
        nextButton.SetActive(false);

        string dir = Path.Combine(DataManager.directoryAPROMData, AppData.selectedMechanism + ".csv");
        if (!Directory.Exists(DataManager.directoryAPROMData)) Directory.CreateDirectory(DataManager.directoryAPROMData);
        if (!File.Exists(dir)) File.WriteAllText(dir, "datetime,promTmin,promTmax,aromTmin,aromTmax\n", Encoding.UTF8);

        int mechanismIndex = Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism);
        angLimit = (mechanismIndex != 4) ? AppData.offsetAtNeutral[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)] : 100.42f;
        
        aromSlider.Setup(-angLimit, angLimit, AppData.oldAROM.aromTmin, AppData.oldAROM.aromTmax);
        aromSlider.minAng = aromSlider.maxAng = 0;

        bool isSpecialCase = (mechanismIndex == 4);
        cText.gameObject.SetActive(isSpecialCase);
        rText.gameObject.SetActive(true);
        lText.gameObject.SetActive(true);
        cText.text = isSpecialCase ? "Closed" : "";

        (_rinx, _linx) = AppData.trainingSide == "right" ? (1, 0) : (0, 1);
        rText.text = DirectionText[mechanismIndex][_rinx];
        lText.text = DirectionText[mechanismIndex][_linx];

        _tmin = _tmax = 180f;
        _state = AssessStates.INIT;
        UpdateGUI();
        PlutoComm.setControlType("NONE");
    }

    public void OnStartButtonClick()
    {
        startAssessment();
        startButton.SetActive(false);
        nextButton.SetActive(true);
    }

    void Update()
    {
        if (!isSelected) return;

        if (_state == AssessStates.INIT && !isInteractable)
        {
            AppData.oldAROM = new ROM(AppData.selectedMechanism);
            PlutoComm.OnButtonReleased += () => isButtonPressed = true;
            isInteractable = true;
        }

        startButton.SetActive(true);
        prommin = AppData.promTmin;
        prommax = AppData.promTmax;

        if (isButtonPressed || Input.GetKeyDown(KeyCode.Return))
        {
            startAssessment();
            isButtonPressed = false;
        }

        UpdateRelaxText();
        UpdateGUI();
    }

    private void UpdateRelaxText()
    {
        int mechanismIndex = Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism);
        bool isSpecialCase = (mechanismIndex == 4);
        float minCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldAROM.aromTmin * 6f);
        float maxCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldAROM.aromTmax * 6f);
        relaxText.color = isRestarting ? Color.red : Color.white;
        relaxText.text = isRestarting ? "AROM Should not Exceed PROM. Please REDO PROM AGAIN" :
            isSpecialCase ? $"Prev AROM: {minCM:0.0}cm : {maxCM:0.0}cm (Aperture: {Mathf.Abs(maxCM - minCM):0.0}cm)" :
            $"Prev AROM: {(int)AppData.oldAROM.aromTmin} : {(int)AppData.oldAROM.aromTmax} ({(int)(AppData.oldAROM.aromTmax - AppData.oldAROM.aromTmin)}°)";
    }

    private void CheckAromLimits()
    {
        if (aromSlider._currePostion.value <= prommin || aromSlider._currePostion.value >= prommax)
        {
            RestartAssessment();
            isRestarting = true;
        }
        else
        {
            aromSlider.UpdateMinMaxvalues = true;
        }
    }

    private void RestartAssessment()
    {
        InitializeAssessment();
    }
    
    public void OnNextButtonClick()
    {
        CheckAromLimits();
        onSavePressed();
        nextButton.SetActive(false);
    }

    public void startAssessment()
    {
        _state = AssessStates.ASSESS;
        nextButton.SetActive(false);
        startButton.SetActive(false);
        aromSlider.startAssessment(PlutoComm.angle);
        aromSlider.UpdateMinMaxvalues = true;
    }

    public void onSavePressed()
    {
        _tmin = aromSlider.minAng;
        _tmax = aromSlider.maxAng;
        AppData.newAROM = new ROM(AppData.promTmin, AppData.promTmax, _tmin, _tmax, AppData.selectedMechanism, true);
        UpdateRelaxText();
        nextButton.SetActive(false);
        aromSlider.UpdateMinMaxvalues = false;

        if (gameData.isPROMcompleted && gameData.isAROMcompleted)
        {
            gameData.setNeutral = true;
            SceneManager.LoadScene("choosegame");
        }
    }

    private void UpdateGUI()
    {
        JointAngle.text = Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) != 4 ? 
            PlutoComm.angle.ToString("0.0") :
            "Aperture " + Mathf.Abs(Mathf.Deg2Rad * PlutoComm.angle * 6f).ToString("0.0") + "cm";
    }
}
