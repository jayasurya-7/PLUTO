using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TS.DoubleSlider;


public class PromWF_Scn_Hndlr_newUI : MonoBehaviour {

    enum AssessStates
    {
        INIT = 0,    
        ASSESS = 1,
        RELAX = 2
    };
    bool assessmentSaved;
    private bool isPaused = false;

    public TMP_Text lText;
    public TMP_Text rText;

    public TMP_Text cText;
    //public TMP_Text statusText;
    public TMP_Text relaxText;

    public TMP_Text JointAngle;

    public TMP_Text JointAngleHoc;

    bool AssessmentValid;

    private int _stpCount;
    private int _stpCountTh = 00;
    private float _lowSpdTh = 20f;
    private float _tmin, _tmax;
    private float _tmin1, _tmax1;


    private double _strttime;
    private double _initDur = 0f;
    private double _assessDur = 180f;
    private double _relaxDur = 3.0f;

    // Assessment torque trajectory
    private float _winT =0;
    private float _freq = 0.1f;
    public static float _torqAmp = .7f;
    private float _currTorq = 0;
    private double _t;
    private double _prevt;
    private double _dt = 0.01;

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
    public pannelselect PanelControl;


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

    // Use this for initialization
    void Start () {
         InitializeAssessment();
    }
 
  public void InitializeAssessment()
  {       
       
        nextButton.SetActive(false);

         


        AppData.oldPROM = new MechanismData(AppData.selectedMechanism);


   
       
        if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) != 3)
        {
         angLimit = AppData.offsetAtNetral[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)];

        //Debug.Log("prom:" + AppData.oldPROM.tmin + "," + AppData.oldPROM.tmax);
        promSlider.Setup(-angLimit, angLimit, AppData.oldPROM.tmin, AppData.oldPROM.tmax);
        promSlider.minAng = 0;
        promSlider.maxAng =0;
    }

        else{
            float minAng = 0;
            float maxAng = 90;

            //Debug.Log("prom:" + AppData.oldPROM.tmin + "," + AppData.oldPROM.tmax);
            // angLimit = 95.42f;
            angLimit = 140.42f;
            promSlider.Setup(-angLimit, angLimit, AppData.oldPROM.tmin, AppData.oldPROM.tmax);  // Centering the slider
   

        }

        ////ANN
        //AppData.oldANN = new AAN(AppData.subjHospNum,
        //    DataTypeDefinitions.PlutoMechanisms[0][AppData.plutoData.mechIndex]);
        


        if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
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
        // Hide the C Text
        Debug.Log(AppData.side + "Side");
        if (AppData.side == "right")
    
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

        

        // Initialize ROM assessment variable.s
        _stpCount = 0;
        _tmin = 180f;
        _tmax = -180f;
        _tmin1 = 180f;
        _tmax1 = -180f;
 
        

        // Start time.
        _strttime = AppData.CurrentTime;

        // Start data logging
        //string _fname = AppData.GetNewFileName("prom");
        //AppData.StartDataLog(_fname);
     
        // Start state in ASSESS
        _state = AssessStates.INIT;
      
        UpdateGUI();
        PlutoComm.setControlType("NONE");
    }
 
    
    private void DisablePromGameObjects()
    {
        //redoButton.SetActive(true);
        startButton.SetActive(false);
        //aromButton.SetActive(false);
        nextButton.SetActive(false);
        //restartButton.SetActive(false);
        // Add other GameObjects to be disabled if necessary
    }
	
	// Update is called once per frame

    public void OnStartButtonClick()
{
    startAssessment(); // Starts the assessment
    startButton.SetActive(false);
    nextButton.SetActive(true);
    //restartButton.SetActive(true);
}


	void Update () {


        //Debug.Log(_state);

        JointAngle.text = ((int)PlutoComm.angle).ToString();

        //JointAngleHoc.text = ((int)PlutoComm.angle).ToString();
        JointAngleHoc.text =((int) PlutoComm.getHOCDisplay(PlutoComm.angle)).ToString();
        //Debug.Log(AppData.plutoData.angle);

        if (isSelected)
        {
            Debug.Log(isSelected + " value");
            if(PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) ==3)
            {
                CurrPositioncursorHoc.SetActive(true);
                CurrPositioncursor.SetActive(true);
            }
            else{
                 CurrPositioncursor.SetActive(true);
            }
       
               
            _t = AppData.CurrentTime - _strttime;
            switch (_state)
            {

                case AssessStates.INIT:
                    //redoButton.SetActive(false);
                    startButton.SetActive(true);
                    //restartButton.SetActive(false);

                    //aromButton.SetActive(false);
                    
                    if ( Input.GetKeyDown(KeyCode.Return))
                    {
                        nextButton.SetActive(true);

                        startAssessment();
                    }
                    //Debug.Log("mech No:" + PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism));
                    if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
                    {
                        // Convert tmin and tmax from degrees to centimeters
                        float max = AppData.oldPROM.tmax/2;
                   float apertureMinCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.tmin * 6f);
                   float apertureMaxCM = Mathf.Abs(Mathf.Deg2Rad *  max * 6f);
                        //Debug.Log("time min: " + AppData.oldPROM.tmin);
                        //Debug.Log("time max: " + AppData.oldPROM.tmax);

                        // Update the relaxText with the aperture values in centimeters
                        relaxText.text = "Prev Prom: " + apertureMinCM.ToString("0.0") + "cm : " + apertureMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(apertureMaxCM - apertureMinCM).ToString("0.0") + "cm)";
                    }
                    else{
                    relaxText.text = "Prev PROM: " + (int)AppData.oldPROM.tmin + " : " + (int)AppData.oldPROM.tmax + " (" + (int)(AppData.oldPROM.tmax - AppData.oldPROM.tmin) + "°)";
                    }
                    break;
                case AssessStates.ASSESS:
                    //nextButton.SetActive(true);
                    
                    startButton.SetActive(false);
                    //restartButton.SetActive(true);


                    assessmentSaved = false;
                    relaxText.text = "Prev PROM: " + (int)AppData.oldPROM.tmin + " : " + (int)AppData.oldPROM.tmax + " (" + (int)(AppData.oldPROM.tmax - AppData.oldPROM.tmin) + "°)";

                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        //Debug.Log("Hello");
                        AssessmentValid = true;
                        _state = AssessStates.RELAX;
                        //OnNextButtonClick();
                                nextButton.SetActive(false);
                                //go to arom;
                                DisablePromGameObjects();
                        // PanelControl.SelectAROM();  
                        //Debug.Log("AROM Started");

                                //Debug.Log("go to arom");
                    
                    }



                    break;
                case AssessStates.RELAX:
              

                    if (AssessmentValid)
                    {
                        Debug.Log("Assessment validated :" + AssessmentValid);

                        if (!assessmentSaved)
                        {

                            relaxText.text = "Press ENTER to Save";
                            // relaxText.color = Color.green;

                            if (Input.GetKeyDown(KeyCode.Return))
                            {
                                OnNextButtonClick();
                            }
                            if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
                           {

                            float apertureMinCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.tmin * 6f);
                            float apertureMaxCM = Mathf.Abs(Mathf.Deg2Rad * -AppData.oldPROM.tmax * 6f);
                            float currentMinCM = Mathf.Abs(Mathf.Deg2Rad * promSlider.minAng * 6f);
                            float currentMaxCM = Mathf.Abs(Mathf.Deg2Rad * -promSlider.maxAng * 6f);

            

                            relaxText.text = "Assessment Completed \n " + "Prev PROM: " + apertureMinCM.ToString("0.0") + "cm : " + apertureMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(apertureMaxCM - apertureMinCM).ToString("0.0") + "cm)\n"+
                                "Current PROM: " + currentMinCM.ToString("0.0") + "cm : " + currentMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(currentMaxCM - currentMinCM).ToString("0.0") + "cm)\n";
                           }
                           else
                            {
                            relaxText.text = "Assessment Completed \n " + "Prev PROM: " + (int)AppData.oldPROM.tmin + " : " + (int)AppData.oldPROM.tmax + " (" + (int)(AppData.oldPROM.tmax - AppData.oldPROM.tmin) + "°)\n" +
                                "Currentt PROM: " + (int)promSlider.minAng + " : " + (int)promSlider.maxAng + " (" + (int)(promSlider.maxAng - promSlider.minAng) + "°)\n";
                            }
                        }
       
                        else
                        {
                            
                            if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
                            {
                                float currentMinCM = Mathf.Abs(Mathf.Deg2Rad * _tmin * 6f);
                                float currentMaxCM = Mathf.Abs(Mathf.Deg2Rad * -_tmax * 6f);

                                relaxText.text = "Assessment Completed \n" +
                                "Current PROM: " + currentMinCM.ToString("0.0") + "cm : " + currentMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(currentMaxCM - currentMinCM).ToString("0.0") + "cm)\n";
                            }
                            else
                            {
                            relaxText.text = relaxText.text = "Assessment Completed \n " +
                                "Currentt PROM: " + (int)_tmin + " : " + (int)_tmax + " (" + (int)(_tmax - _tmin) + "°)\n";
                            }
                            if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) < 4)
                            {

                                //AANScene.SetActive(true);

                            }
                            

                        }
                    }
                    else
                    {
                        
                        relaxText.text = "PROM should be greater than AROM\n " +
                            "Press ENTER Redo Assesment ";
           
                    }
                    break;

                   
            }


            UpdateGUI();
        }
        else
        {
            Debug.Log(" PROM deselected");

        

            if(PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
            {
            float apertureMinCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.tmin * 6f);
            float apertureMaxCM = Mathf.Abs(Mathf.Deg2Rad * AppData.oldPROM.tmax * 6f);
            float currentMinCM = Mathf.Abs(Mathf.Deg2Rad * promSlider.minAng * 6f);
            float currentMaxCM = Mathf.Abs(Mathf.Deg2Rad * promSlider.maxAng * 6f);
            relaxText.text = "Assessment Completed \n " + "Prev PROM: " + apertureMinCM.ToString("0.0") + "cm : " + apertureMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(apertureMaxCM - apertureMinCM).ToString("0.0") + "cm)\n"+
                                "Current PROM: " + currentMinCM.ToString("0.0") + "cm : " + currentMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(currentMaxCM - currentMinCM).ToString("0.0") + "cm)\n";
            }
            else{
            relaxText.text = "Assessment Completed \n "+"Prev PROM: " + (int)AppData.oldPROM.tmin + " : " + (int)AppData.oldPROM.tmax + " (" + (int)(AppData.oldPROM.tmax - AppData.oldPROM.tmin) + "°) ||" +
                              "Current PROM: " + (int)promSlider.minAng + " : " + (int)promSlider.maxAng + " (" + (int)(promSlider.maxAng - promSlider.minAng) + "°)\n";
            }
            //Debug.Log(" PROM deselected");
            //CurrPositioncursor.SetActive(false);
        
        }
    }
    public void OnRedopromButtonClick()
    {
        // Call the InitializeAssessment method to reset and reinitialize the assessment
        PanelControl.SelectpROM();  
        //nextButton.SetActive(true);
        InitializeAssessment();
        Debug.Log("Assessment Restarted");
        Start();
    }


   public void OnNextButtonClick()
   {
    OnSaveClick();
        Debug.Log("AROM moved");
    PanelControl.SelectAROM(); // Move to AROM panel
    DisablePromGameObjects();
    _state = AssessStates.RELAX;

   }

   public void OnrestartButtonClick()
   {
    //startAssessment();
    Start();
   }


    public void OnSaveClick()
    {
        nextButton.SetActive(false);
        //aromButton.SetActive(false);
        _tmin = promSlider.minAng;
        _tmax = promSlider.maxAng;
        assessmentSaved = true;
        Debug.Log("Onsave : " + _tmin+" , "+ _tmax);
        AppData.newPROM = new MechanismData(AppData.side, _tmin, _tmax,
            AppData.selectedMechanism, true);
        


        promSlider.UpdateMinMaxvalues = false;
        CurrPositioncursor.SetActive(false);
        CurrPositioncursorHoc.SetActive(false);
       
       
        promSlider.minAng = AppData.newPROM.tmin; 

        nextButton.SetActive(false);
       
        if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
            {
                float currentMinCM = Mathf.Abs(Mathf.Deg2Rad * _tmin * 6f);
                float currentMaxCM = Mathf.Abs(Mathf.Deg2Rad * -_tmax * 6f);

                relaxText.text = "Assessment Completed \n" +
                                "Current PROM: " + currentMinCM.ToString("0.0") + "cm : " + currentMaxCM.ToString("0.0") + "cm (Aperture: " + Mathf.Abs(currentMaxCM - currentMinCM).ToString("0.0") + "cm)\n";
            }
        else
        {
        relaxText.text = "Assessment Completed \n " +
        "Current PROM: " + (int)_tmin + " : " + (int)_tmax + " (" + (int)(_tmax - _tmin) + " °)\n";
        }
       
        //AppData.oldPROM = null;
       // AppData.StopLogging();
    }
       



     public void startAssessment()
    {
        _state = AssessStates.ASSESS;

        {
         promSlider.startAssessment(PlutoComm.angle);

        promSlider.UpdateMinMaxvalues = true;
        }
        

    }

    bool validAssessment()
    {
        AppData.oldAROM = new AROM(AppData.selectedMechanism);
        if (_tmin <= AppData.oldAROM.tmin && _tmax >= AppData.oldAROM.tmax)
        {
            return true;
        }
        else
            return false;
    }
    


    private float window(float currt, float tT, float rT)
    {
        if (currt < 0)
        {
            return 0.0f;
        } else if (currt >= 0 && currt < (tT + rT))
        {
            return Mathf.Min(1.0f, currt / rT);
        } else if (currt >= (tT + rT) && currt < (tT + 2 * rT))
        {
            return 1.0f + (tT + rT - currt) / rT;
        } else
        {
            return 0.0f;
        }
    }

    void OnApplicationQuit()
    {
        //AppData.WriteSessionInfo("Quiting Application from PROM Assessment scene.");
       JediComm.Disconnect();
    }

    public void On_Back_Click()
    {
        //AppData.WriteSessionInfo("Back to Game menu.");
        SceneManager.LoadScene(2);
    }
    
    private void UpdateGUI()
    {
        UpdateStatusText();
    }


    public void OnFinishPressed()
    {
        _state = AssessStates.RELAX;
    }
    private void UpdateStatusText()
    {
        if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) != 3)
        {
            JointAngle.text =  (PlutoComm.angle).ToString("0.0") ;
        }
        else
            JointAngle.text = "Aperture"   + Mathf.Abs((Mathf.Deg2Rad * PlutoComm.angle*6f)).ToString("0.0") + "cm";

            JointAngleHoc.text = "Aperture"   + Mathf.Abs((Mathf.Deg2Rad * PlutoComm.angle*6f)).ToString("0.0") + "cm";

        //Debug.Log(AppData.plutoData.angle);
        if (AppData.count[1]++ > AppData.Th[1])
        {
          //  statusText.text = "FR: " + ((int)MySerialThread.framerate).ToString();
            AppData.count[1] = 0;

        }
    }


}
