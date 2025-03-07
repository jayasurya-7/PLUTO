using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TS.DoubleSlider;
using System.IO;


public class PROMsceneHandler : MonoBehaviour {

    enum AssessStates
    {
        INIT = 0,    
        ASSESS = 1
    };
    bool assessmentSaved;
    private bool isPaused = false;
    private bool isButtonPressed = false;
    public TMP_Text lText;
    public TMP_Text rText;
    public TMP_Text insText;
    public TMP_Text cText;
    //public TMP_Text statusText;
    public TMP_Text relaxText;

    public TMP_Text JointAngle;

    public TMP_Text JointAngleHoc;

    bool AssessmentValid;

    private float _tmin, _tmax, _tmin1, _tmax1;

    public GameObject nextButton;
    public GameObject startButton;
    public GameObject  CurrPositioncursor;
    public GameObject  CurrPositioncursorHoc;
    private AssessStates _state;

    private float angLimit;
    public DoubleSlider promSlider;

    public DoubleSlider promSliderHOC;

    //public DoubleSlider promSlider1;

    public bool isSelected = false;
    public bool inst = false;
    public assessmentSceneHandler panelControl;


    private List<string[]> DirectionText = new List<string[]>
    {
        new string[] { "Flexion", "Extension" },
        new string[] { "Ulnar Dev.", "Radial Dev."},
        new string[] { "Pronation", "Supination" },
        new string[]{"Open", "Open"},
        new string[] {"",""},
        new string[] {"",""}
    };

    private int _linx, _rinx;
    internal bool interactable;

    void Start () {
         InitializeAssessment();

    }
 
  public void InitializeAssessment()
  {       
        nextButton.SetActive(false);
        AppData.oldPROM = new ROM(AppData.selectedMechanism);

        if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) != 4)
        {
         angLimit = AppData.offsetAtNeutral[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)];
        promSlider.Setup(-angLimit, angLimit, AppData.oldPROM.promTmin, AppData.oldPROM.promTmax);
        promSlider.minAng = 0;
        promSlider.maxAng =0;
        }

        else{

            angLimit = 100.42f;
            promSlider.Setup(-angLimit, angLimit, AppData.oldPROM.promTmin, AppData.oldPROM.promTmax);  // Centering the slider
        }

        if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 4)
    {
        cText.gameObject.SetActive(true); // Show the C Text
        rText.gameObject.SetActive(true); 
        
        lText.gameObject.SetActive(true); 
        
        cText.text = "Closed"; // Set the C Text in the center
    }
        else
        {
            cText.gameObject.SetActive(false);
        }
        if (AppData.trainingSide == "right")
        {
            _rinx = 1;
            _linx = 0;
        }
        else
        {
            _rinx = 0;
            _linx = 1;
        }
    rText.gameObject.SetActive(true);
    lText.gameObject.SetActive(true);
    rText.text = DirectionText[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)][_rinx];
    lText.text = DirectionText[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)][_linx];

        _tmin = 180f;
        _tmax = -180f;
        _tmin1 = 180f;
        _tmax1 = -180f;
 

        _state = AssessStates.INIT;
      
        UpdateGUI();
        PlutoComm.setControlType("NONE");
    }
 
    
    private void DisablePromGameObjects()
    {
        startButton.SetActive(false);
        nextButton.SetActive(false);
    }
	
    public void OnStartButtonClick()
{
    startAssessment(); 
    startButton.SetActive(false);
    nextButton.SetActive(true);
}


	void Update () {


        JointAngle.text = ((int)PlutoComm.angle).ToString();
        JointAngleHoc.text =((int) PlutoComm.getHOCDisplay(PlutoComm.angle)).ToString();

        if (isSelected)
        {
            if(Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) ==4)
            {
                CurrPositioncursorHoc.SetActive(true);
                CurrPositioncursor.SetActive(true);
            }
            else{
                 CurrPositioncursor.SetActive(true);
            }
       
        

            switch (_state)
            {

                case AssessStates.INIT:

                    startButton.SetActive(true);
                    PlutoComm.OnButtonReleased += OnPlutoButtonReleased;
               

                    if (isButtonPressed || Input.GetKeyDown(KeyCode.Return))
                    {
                        
                        startAssessment();
                        isButtonPressed = false;
                    }
                    if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 4)
                    {
                        // Convert tmin and tmax from degrees to centimeters
                        float apertureMinCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.promTmin * 6f);
                        float apertureMaxCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.promTmax * 6f);
                        relaxText.text = "Prev Prom: " + apertureMinCM.ToString("0.0") + "cm : " + apertureMaxCM.ToString("0.0")
                            + "cm (Aperture: " + Mathf.Abs(apertureMaxCM - apertureMinCM).ToString("0.0") + "cm)";
                    }
                    else{
                    relaxText.text = "Prev PROM: " + (int)AppData.oldPROM.promTmin + " : " + 
                            (int)AppData.oldPROM.promTmax + " (" + (int)(AppData.oldPROM.promTmax - AppData.oldPROM.promTmin) + "°)";
                    }
                    break;
                case AssessStates.ASSESS:
                    
                    startButton.SetActive(false);
                    _tmin = promSlider.minAng;
                    _tmax = promSlider.maxAng;
                    

                     
                        if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) != 4)
                        {

                            relaxText.text = "Prev PROM: " + (int)AppData.oldPROM.promTmin + " : " + (int)AppData.oldPROM.promTmax 
                                + " (" + (int)(AppData.oldPROM.promTmax - AppData.oldPROM.promTmin) + "°)" ;
                        }
                        else
                        {
                            // Convert tmin and tmax from degrees to centimeters
                            float max = AppData.oldPROM.promTmax / 2;
                            float apertureMinCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.promTmin * 6f);
                            float apertureMaxCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.promTmax * 6f);
                            relaxText.text = "Prev Prom: " + apertureMinCM.ToString("0.0") + "cm : " + apertureMaxCM.ToString("0.0")
                                + "cm (Aperture: " + Mathf.Abs(apertureMaxCM - apertureMinCM).ToString("0.0") + "cm)";

                        }
                        nextButton.SetActive(true);

                        if (isButtonPressed || Input.GetKeyDown(KeyCode.Return))
                        {
                            OnNextButtonClick();
                            nextButton.SetActive(false);
                            DisablePromGameObjects();
                            isButtonPressed = false;
                        }

                    

                    break;
              
            }
            UpdateGUI();
        }
        else
        {
            if(Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 4)
            {
            float apertureMinCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.promTmin * 6f);
            float apertureMaxCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.promTmax * 6f);
            float currentMinCM = Mathf.Abs(Mathf.Deg2Rad * promSlider.minAng * 6f);
            float currentMaxCM = Mathf.Abs(Mathf.Deg2Rad * promSlider.maxAng * 6f);
            relaxText.text = "Assessment Completed \n " + "Prev PROM: " + apertureMinCM.ToString("0.0") + "cm : " +
                    apertureMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(apertureMaxCM - apertureMinCM).ToString("0.0") + "cm)\n"+
                                "Current PROM: " + currentMinCM.ToString("0.0") + "cm : " + currentMaxCM.ToString("0.0") + "cm (Aperture: " 
                                + Mathf.Abs(currentMaxCM - currentMinCM).ToString("0.0") + "cm)\n";
            }
            else{
            relaxText.text = "Assessment Completed \n "+"Prev PROM: " + (int)AppData.oldPROM.promTmin + " : " + (int)AppData.oldPROM.promTmax + 
                    " (" + (int)(AppData.oldPROM.promTmax - AppData.oldPROM.promTmin) + "°) ||" + "Current PROM: " + (int)promSlider.minAng + " : "
                    + (int)promSlider.maxAng + " (" + (int)(promSlider.maxAng - promSlider.minAng) + "°)\n";
            }
        
        }
    }
    

    public void OnRedoPromClick()
    {
        _state = AssessStates.INIT;
        AssessmentValid = false;
        isButtonPressed = false;
        gameData.isAROMcompleted = false;
        // Reinitialize the assessment process
        InitializeAssessment();

        UpdateGUI();
        panelControl.SelectpROM();
        Debug.Log("Redo PROM: Reset to INIT state.");
    }

    public void OnPlutoButtonReleased()
    {
            isButtonPressed = true;
        
    }


    public void OnNextButtonClick()
   {
    OnSaveClick();
    panelControl.SelectAROM(); 
    DisablePromGameObjects();

   }
    public void OnrestartButtonClick()
   {
    Start();
   }


    public void OnSaveClick()
    {
        nextButton.SetActive(false);
        _tmin = promSlider.minAng;
        _tmax = promSlider.maxAng;
        assessmentSaved = true;
        Debug.Log("Onsave : " + _tmin+" , "+ _tmax);
        gameData.isPROMcompleted= true;
        AppData.promTmin= _tmin;
        AppData.promTmax= _tmax;

        promSlider.UpdateMinMaxvalues = false;
        CurrPositioncursor.SetActive(false);
        CurrPositioncursorHoc.SetActive(false);
       
       
        promSlider.minAng = AppData.promTmin; 

        nextButton.SetActive(false);
       
        if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 4)
            {
                float currentMinCM = Mathf.Abs(Mathf.Deg2Rad * _tmin * 6f);
                float currentMaxCM = Mathf.Abs(Mathf.Deg2Rad * -_tmax * 6f);

                relaxText.text = "Assessment Completed \n" +
                                "Current PROM: " + currentMinCM.ToString("0.0") + "cm : " + currentMaxCM.ToString("0.0")
                                + "cm (Aperture: " + Mathf.Abs(currentMaxCM - currentMinCM).ToString("0.0") + "cm)\n";
            }
        else
        {
        relaxText.text = "Assessment Completed \n " +
        "Current PROM: " + (int)_tmin + " : " + (int)_tmax + " (" + (int)(_tmax - _tmin) + " °)\n";
        }
    }
       



     public void startAssessment()
    {
        _state = AssessStates.ASSESS;
        promSlider.minAng = 0;
        promSlider.maxAng = 0;
       
         promSlider.startAssessment(PlutoComm.angle);
            promSlider.UpdateMinMaxvalues = true;
        
    }

    void OnApplicationQuit()
    {
       JediComm.Disconnect();
    }

    
    private void UpdateGUI()
    {
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (Array.IndexOf(PlutoComm.MECHANISMS, AppData.selectedMechanism) != 4)
        {
            JointAngle.text =  (PlutoComm.angle).ToString("0.0") ;
        }
        else {
            JointAngle.text = "Aperture" + Mathf.Abs((Mathf.Deg2Rad * PlutoComm.angle * 6f)).ToString("0.0") + "cm";

            JointAngleHoc.text = "Aperture" + Mathf.Abs((Mathf.Deg2Rad * PlutoComm.angle * 6f)).ToString("0.0") + "cm";
        }

    }


}
