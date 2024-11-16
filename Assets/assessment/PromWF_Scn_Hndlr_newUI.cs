using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TS.DoubleSlider;
using XCharts.Runtime;

public class xPromWF_Scn_Hndlr_newUI : MonoBehaviour
{

    enum AssessStates
    {
        INIT = 0,
        ASSESS = 1,
        RELAX = 2
    };
    bool assessmentSaved;
    public GameObject redoButton;
    public TMP_Text lText;
    public TMP_Text rText;
    public TMP_Text relaxText;

    public TMP_Text JointAngle;

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
    private float _winT = 0;
    private float _freq = 0.1f;
    public static float _torqAmp = .7f;
    private float _currTorq = 0;
    private double _t;
    private double _prevt;
    private double _dt = 0.01;

    public GameObject nextButton;
    public GameObject startButton;
    //public GameObject AANScene;
    private AssessStates _state;

    private float angLimit;
    public DoubleSlider promSlider;
    public bool inputPressed = false;
    public bool isSelected = true;

    static public float[] offsetAtNetral = new float[] { 70, 70, 90, 0, 270 / 2, 270 / 2 };

    private List<string[]> DirectionText = new List<string[]>
    {
        new string[] { "Flexion", "Extension" },
        new string[] { "Ulnar Dev.", "Radial Dev."},
        new string[] { "Pronation", "Supination" },
        new string[]{"Close", "Open"},
        new string[] {"",""},
        new string[] {"",""}
    };

    private int _linx, _rinx;

    // Use this for initialization
    void Start()
    {
        AppData.initializeStuff();
        AppData.selectedMechanism = "HOC";
        nextButton.SetActive(false);


        AppData.oldPROM = new MechanismData(AppData.selectedMechanism);

        // Attach PLUTO button event
        PlutoComm.OnButtonReleased += OnPlutoButtonReleased;

        StartCoroutine(DelayedInitialization());
    }

    // Update is called once per frame
    void Update()
    {



        JointAngle.text = ((int)PlutoComm.angle).ToString();


        switch (_state)
        {

            case AssessStates.INIT:
                redoButton.SetActive(false);
                startButton.SetActive(true);


                if (Input.GetKeyDown(KeyCode.K))
                {

                    startAssessment();
                    inputPressed = false;

                }
                relaxText.text = "Press ENTER to start assessment \n" + "Prev ROM: " + (int)AppData.oldPROM.tmin + " : " + (int)AppData.oldPROM.tmax + " (" + (int)(AppData.oldPROM.tmax - AppData.oldPROM.tmin) + "°)";


                break;
            case AssessStates.ASSESS:

                startButton.SetActive(false);

                Debug.Log(_state);

                Debug.Log("Assessment started");
                assessmentSaved = false;
                if (Input.GetKeyDown(KeyCode.K))
                {
                    Debug.Log("Hello");
                    AssessmentValid = true;
                    _state = AssessStates.RELAX;
                    inputPressed = false;
                }
                relaxText.text = "Assessing \n " +
                    "Press ENTER to finish";
                break;
            case AssessStates.RELAX:
                // Update text
                //SendToRobot.ControlParam(AppData.plutoData.mechs[AppData.plutoData.mechIndex], ControlType.NONE, true, false);



                //Debug.Log(_state);
                redoButton.SetActive(true);
                Debug.Log("Relaxed");
                //if (AssessmentValid)
                //{
                //    if (!assessmentSaved)
                //    {
                //        nextButton.SetActive(true);
                //        relaxText.text = "Press ENTER to Save";
                //        // relaxText.color = Color.green;
                //        relaxText.text = "Assessment Completed \n " + "Prev ROM: " + (int)AppData.oldPROM.tmin + " : " + (int)AppData.oldPROM.tmax + " (" + (int)(AppData.oldPROM.tmax - AppData.oldPROM.tmin) + "°)\n" +
                //            "Currentt ROM: " + (int)promSlider.minAng + " : " + (int)promSlider.maxAng + " (" + (int)(promSlider.maxAng - promSlider.minAng) + "°)\n";
                //        if (inputPressed || Input.GetKeyDown(KeyCode.K))
                //        {
                //            OnSaveClick();
                //            inputPressed = false;

                //        }
                //    }
                //    else
                //    {
                //    Debug.Log("Saved");
                //        nextButton.SetActive(false);
                //        redoButton.SetActive(true);
                //        relaxText.text = relaxText.text = "Assessment Completed \n " +
                //            "Currentt PROM: " + (int)_tmin + " : " + (int)_tmax + " (" + (int)(_tmax - _tmin) + "°)\n";
                //        if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) < 4)
                //        {

                //            //AANScene.SetActive(true);

                //        }
                //        if (Input.GetKeyDown(KeyCode.Return))
                //        {

                //            //go to arom;
                //            //PanelControl.SelectAROM();
                //            Debug.Log("go to arom");
                //            inputPressed = false;


                //        }

                //    }
                //}
                //else
                //{

                //    relaxText.text = "PROM should be greater than AROM\n " +
                //        "Press ENTER Redo Assessment ";
                //    // relaxText.color = Color.red;
                //    if (Input.GetKeyDown(KeyCode.K))
                //    {
                //        _stpCount = 0;
                //        _tmin = 180f; 
                //        _tmax = -180f;
                //        _tmin1 = 180f;
                //        _tmax1 = -180f;
                //        //  UpdateNewAROMValueDisplay();
                //        OnRedoPressed();
                //        inputPressed = false;

                //    }
                //}
                break;
        }


        UpdateGUI();

    }
    private IEnumerator DelayedInitialization()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Initialization code
        angLimit = offsetAtNetral[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)];

        Debug.Log("pron:" + AppData.oldPROM.tmin + "," + AppData.oldPROM.tmax);
        promSlider.Setup(-angLimit, angLimit, AppData.oldPROM.tmin, AppData.oldPROM.tmax);
        promSlider.minAng = 0;
        promSlider.maxAng = 0;

        if (AppData.subjd.side == "right")
        {
            _rinx = 1;
            _linx = 0;
        }
        else
        {
            _rinx = 0;
            _linx = 1;
        }

        // Uncomment these lines if needed to update text fields based on the direction
        // rText.text = DirectionText[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)][_rinx];
        // Debug.Log(rText.text);
        // Debug.Log(lText.text);
        // lText.text = DirectionText[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)][_linx];

        // Initialize ROM assessment variables
        _stpCount = 0;
        _tmin = 180f;
        _tmax = -180f;
        _tmin1 = 180f;
        _tmax1 = -180f;

        fun();

        // Start time
        _strttime = AppData.CurrentTime;

        _state = AssessStates.INIT;

        UpdateGUI();
        isSelected = true;
        PlutoComm.setControlType("NONE");
    }
    public void fun()
    {
        rText.text = "ok1";
        lText.text = "Makamishi";
    }
    public void OnSaveClick()
    {
        _tmin = promSlider.minAng;
        _tmax = promSlider.maxAng;
        Debug.Log("MinAng:" + _tmin);
        //assessmentSaved = true;
        //AppData.newPROM = new PassiveRangeOfMotion(AppData.subjHospNum, AppData.subjd.side, _tmin, _tmax,
        // DataTypeDefinitions.PlutoMechanisms[0][AppData.plutoData.mechIndex], true);
        // AppData.oldPROM = null;
        Debug.Log("MaxAng:" + _tmax);

        nextButton.SetActive(false);
        //aromButton.SetActive(true);
        //// Stop logging
        //relaxText.text = "Assessment Completed \n " +
        //"Current PROM: " + (int)_tmin + " : " + (int)_tmax + " (" + (int)(_tmax - _tmin) + " °)\n";

        //AppData.StopLogging();
    }
    public void startAssessment()
    {

        _state = AssessStates.ASSESS;
        promSlider.startAssessment(PlutoComm.angle);

        promSlider.UpdateMinMaxvalues = true;

        Debug.Log("promSlider:" + promSlider.UpdateMinMaxvalues);

    }

    bool validAssessment()
    {

        if (_tmin <= AppData.oldPROM.tmin && _tmax >= AppData.oldPROM.tmax)
        {
            return true;
        }
        else
            return false;
    }
    public void OnPlutoButtonReleased()
    {
        inputPressed = true;
    }
    public void OnRedoPressed()
    {
        _state = AssessStates.ASSESS;
        promSlider.Setup(-angLimit, angLimit, AppData.oldPROM.tmin, AppData.oldPROM.tmax);
        promSlider.minAng = 0;
        promSlider.maxAng = 0;
        _stpCount = 0;
        _tmin = 180f;
        _tmax = -180f;
        _tmin1 = 180f;
        _tmax1 = -180f;

        redoButton.SetActive(false);

    }
    private float window(float currt, float tT, float rT)
    {
        if (currt < 0)
        {
            return 0.0f;
        }
        else if (currt >= 0 && currt < (tT + rT))
        {
            return Mathf.Min(1.0f, currt / rT);
        }
        else if (currt >= (tT + rT) && currt < (tT + 2 * rT))
        {
            return 1.0f + (tT + rT - currt) / rT;
        }
        else
        {
            return 0.0f;
        }
    }

    void OnApplicationQuit()
    {
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


            JointAngle.text = (PlutoComm.angle).ToString("0.0");

        }
        else
            JointAngle.text = "Aperture" + Mathf.Abs((Mathf.Deg2Rad * PlutoComm.angle * 6f)).ToString("0.0") + "cm";

        //Debug.Log(AppData.plutoData.angle);
        if (AppData.count[1]++ > AppData.Th[1])
        {
            //  statusText.text = "FR: " + ((int)MySerialThread.framerate).ToString();
            AppData.count[1] = 0;
        }
    }
}








//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;

//public class xPromWF_Scn_Hndlr : MonoBehaviour
//{

//    enum AssessStates
//    {
//        INIT = 0,
//        ASSESS = 1,
//        RELAX = 2
//    };
//    bool assessmentSaved;
//    public GameObject redoAROM;
//    public GameObject newpromLR;
//    public GameObject newpromLR1;
//    public GameObject oldpromLR;
//    public GameObject oldpromLR1;
//    public GameObject redoButton;
//    public Text lText;
//    public Text rText;
//    public Text statusText;
//    public Text relaxText;

//    public Text JointAngle;

//    bool AssessmentValid;

//    private int _stpCount;
//    private int _stpCountTh = 00;
//    private float _lowSpdTh = 20f;
//    private float _tmin, _tmax;
//    private float _tmin1, _tmax1;


//    private double _strttime;
//    private double _initDur = 0f;
//    private double _assessDur = 180f;
//    private double _relaxDur = 3.0f;

//    // Assessment torque trajectory
//    private float _winT = 0;
//    private float _freq = 0.1f;
//    public static float _torqAmp = .7f;
//    private float _currTorq = 0;
//    private double _t;
//    private double _prevt;
//    private double _dt = 0.01;

//    public GameObject nextButton;
//    public GameObject AANScene;
//    private AssessStates _state;

//    private List<string[]> DirectionText = new List<string[]>
//    {
//        new string[] { "Flexion", "Extension" },
//        new string[] { "Ulnar Dev.", "Radial Dev."},
//        new string[] { "Pronation", "Supination" },
//        new string[]{"Close", "Open"},
//        new string[] {"",""},
//        new string[] {"",""}
//    };

//    private int _linx, _rinx;

//    // Use this for initialization
//    void Start()
//    {
//        AppData.initializeStuff();
//        AppData.selectedMechanism = "HOC";

//        //AppData.WriteSessionInfo("Entered PROM Assessment scene.");
//        nextButton.SetActive(false);
//        AANScene.SetActive(false);


//        AppData.oldPROM = new MechanismData(AppData.selectedMechanism);
//        // Draw old AROM
//        oldpromLR.GetComponent<OldAROM>().CreatePoints(AppData.oldPROM.tmin, AppData.oldPROM.tmax);
//        if (AppData.plutoData.mechIndex == 3)
//        {
//            oldpromLR1.GetComponent<OldAROM>().CreatePoints(-AppData.oldPROM.tmax, -AppData.oldPROM.tmin);
//        }



//        if (AppData.subjd.side == "RIGHT")
//        {
//            _rinx = 1;
//            _linx = 0;
//        }
//        else
//        {
//            _rinx = 0;
//            _linx = 1;
//        }
//        rText.text = DirectionText[AppData.plutoData.mechIndex][_rinx];
//        lText.text = DirectionText[AppData.plutoData.mechIndex][_linx];

//        // Initialize ROM assessment variable.s
//        _stpCount = 0;
//        _tmin = 180f;
//        _tmax = -180f;
//        _tmin1 = 180f;
//        _tmax1 = -180f;



//        // Start time.
//        _strttime = AppData.CurrentTime;



//        // Start state in ASSESS
//        _state = AssessStates.INIT;

//        UpdateGUI();
//        PlutoComm.setControlType("NONE");
//    }

//    // Update is called once per frame
//    void Update()
//    {


//        _t = AppData.CurrentTime - _strttime;
//        switch (_state)
//        {
//            case AssessStates.INIT:
//                redoAROM.SetActive(false);

//                if (AppData.inputPressed() || Input.GetKeyDown(KeyCode.Return))
//                {

//                    _state = AssessStates.ASSESS;
//                }
//                relaxText.text = "Press ENTER to start assessment";
//                relaxText.color = Color.white;
//                // Check if it is assessment time.

//                break;
//            case AssessStates.ASSESS:

//                assessmentSaved = false;
//                if (AppData.inputPressed() || Input.GetKeyDown(KeyCode.Return))
//                {
//                    Debug.Log("Hello");
//                    AssessmentValid = validAssessment();
//                    _state = AssessStates.RELAX;
//                }
//                relaxText.text = "Assessing \n " +
//                    "Press ENTER to finish";
//                relaxText.color = Color.green;

//                // Check if it is assessment time.
//                UpdateNewAROMValueDisplay();


//                break;
//            case AssessStates.RELAX:
//                // Update text
//                //SendToRobot.ControlParam(AppData.plutoData.mechs[AppData.plutoData.mechIndex], ControlType.NONE, true, false);



//                redoButton.SetActive(true);

//                if (AssessmentValid)
//                {
//                    if (!assessmentSaved)
//                    {
//                        relaxText.text = "Press ENTER to Save";
//                        relaxText.color = Color.green;
//                        if (AppData.inputPressed() || Input.GetKeyDown(KeyCode.Return))
//                        {
//                            assessmentSaved = true;
//                            AppData.newPROM = new PassiveRangeOfMotion(AppData.subjHospNum, AppData.subjd.side, _tmin, _tmax,
//                             DataTypeDefinitions.PlutoMechanisms[0][AppData.plutoData.mechIndex], true);
//                            AppData.oldPROM = null;
//                            redoButton.SetActive(false);

//                            // Stop logging
//                            relaxText.text = "PROM Assessment Completed";
//                            nextButton.SetActive(true);
//                            AppData.StopLogging();
//                        }
//                    }
//                    else
//                    {
//                        relaxText.text = "PROM Assessment Completed \n" +
//                            "Press Enter to Return to MAIN MENU";
//                        if (AppData.plutoData.mechIndex < 4)
//                        {

//                            AANScene.SetActive(true);

//                        }
//                        if (AppData.inputPressed() || Input.GetKeyDown(KeyCode.Return))
//                        {

//                            SceneManager.LoadScene("menu_new");

//                        }

//                    }
//                }
//                else
//                {
//                    redoAROM.SetActive(true);
//                    relaxText.text = "PROM should be greater than AROM\n " +
//                        "Press ENTER Redo Assesment ";
//                    relaxText.color = Color.red;
//                    if (AppData.inputPressed() || Input.GetKeyDown(KeyCode.Return))
//                    {
//                        _stpCount = 0;
//                        _tmin = 180f;
//                        _tmax = -180f;
//                        _tmin1 = 180f;
//                        _tmax1 = -180f;
//                        UpdateNewAROMValueDisplay();
//                        OnRedoPressed();
//                    }
//                }
//                break;
//        }


//        UpdateGUI();
//    }



//    bool validAssessment()
//    {
//        if (_tmin <= AppData.oldAROM.tmin && _tmax >= AppData.oldAROM.tmax)
//        {
//            return true;
//        }
//        else
//            return false;
//    }
//    public void OnRedoPressed()
//    {
//        _state = AssessStates.ASSESS;
//        _stpCount = 0;
//        _tmin = 180f;
//        _tmax = -180f;
//        _tmin1 = 180f;
//        _tmax1 = -180f;
//        redoAROM.SetActive(false);
//        redoButton.SetActive(false);
//        //newpromLR.GetComponent<NewAROM>().resetPoint();
//    }
//    private float window(float currt, float tT, float rT)
//    {
//        if (currt < 0)
//        {
//            return 0.0f;
//        }
//        else if (currt >= 0 && currt < (tT + rT))
//        {
//            return Mathf.Min(1.0f, currt / rT);
//        }
//        else if (currt >= (tT + rT) && currt < (tT + 2 * rT))
//        {
//            return 1.0f + (tT + rT - currt) / rT;
//        }
//        else
//        {
//            return 0.0f;
//        }
//    }

//    void OnApplicationQuit()
//    {
//        //AppData.WriteSessionInfo("Quiting Application from PROM Assessment scene.");
//    JediComm.Disconnect();
//    }

//    //private void UpdateNewAROMValueDisplay()
//    //{
//    //    // Debug.Log("UPDATING");
//    //    // Upate current AROM measurement.


//    //    // Update tmin and tmax.
//    //    _tmax = Mathf.Max(_tmax, PlutoComm.angle);
//    //    _tmin = Mathf.Min(_tmin, PlutoComm.angle);
//    //    //Debug.Log(_tmin.ToString() + ", " + _tmax.ToString());
//    //    if (_tmin < _tmax)
//    //    {
//    //        newpromLR.GetComponent<NewAROM>().CreatePoints(_tmin, _tmax);
//    //    }
//    //    if (AppData.plutoData.mechIndex == 3)
//    //    {
//    //        _tmax1 = Mathf.Max(_tmax1, -AppData.plutoData.angle);
//    //        _tmin1 = Mathf.Min(_tmin1, -AppData.plutoData.angle);
//    //        if (_tmin1 < _tmax1)
//    //        {
//    //            newpromLR1.GetComponent<NewAROM>().CreatePoints(_tmin1, _tmax1);
//    //        }
//    //    }


//    //}





//    public void On_Back_Click()
//    {
//        //AppData.WriteSessionInfo("Back to Game menu.");
//        SceneManager.LoadScene(2);
//    }

//    private void UpdateGUI()
//    {
//        UpdateStatusText();
//    }

//    private void UpdateStatusText()
//    {
//        if (AppData.plutoData.mechIndex != 3)
//        {


//            JointAngle.text = "Angle: " + (AppData.plutoData.angle).ToString("0.0") + "° " + DirectionText[AppData.plutoData.mechIndex][AppData.plutoData.angle > 0 ? _rinx : _linx];

//        }
//        else
//            JointAngle.text = "Aperture" + Mathf.Abs((Mathf.Deg2Rad * AppData.plutoData.angle * 6f)).ToString("0.0") + " cm";

//        //Debug.Log(AppData.plutoData.angle);
//        if (AppData.count[1]++ > AppData.Th[1])
//        {
//            //statusText.text = "FR: " + ((int)MySerialThread.framerate).ToString();
//            AppData.count[1] = 0;
//        }
//    }
//}
