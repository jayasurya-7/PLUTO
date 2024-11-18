
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class pannelselect : MonoBehaviour
{

    public Button promButton;

    public Button RedoPRom;
    public Button aromButton;
    public TextMeshProUGUI mechName;
    
    public PromWF_Scn_Hndlr_newUI promHandler;
    public AromWF_Scn_Hndlr_newUI aromHandler;
    public Image promImage;
    public Image aromImage;

    public GameObject[] aromSelected; 
    public GameObject[] promSelected;
    private string mech;
    static int steps = 10;
    public static float[] assistProfile = new float[steps];

    //public ToggleGroup assistTG;


    // Start is called before the first frame update
    void Start()
    {
        AppData.initializeStuff();
        
        SelectpROM();
        mech = AppData.selectedMechanism;
        //UpdateAssistProfile();
        mechName.text = PlutoComm.MECHANISMSTEXT[PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, mech)];
      

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(assistTG.ActiveToggles().FirstOrDefault().gameObject.name);
     
    }

    

    public void writeAssesmentFileAndExit()
    {
        //UpdateAssistProfile();
        SceneManager.LoadScene("choosegame");
        Debug.Log("Wrote successfully");
        //string _fname = Path.Combine(SubjectData.Get_Subj_Assessment_Dir(AppData.subjHospNum), "aan_" + mech + ".csv");
        //using (StreamWriter file = new StreamWriter(_fname, true))
        //{
        //    AppData.dateTime = DateTime.Now.ToString("Dyyyy-MM-ddTHH-mm-ss");
        //    string res = String.Join(",", assistProfile);
        //    file.WriteLine(AppData.dateTime + ", " + AppData.pROM()[0].ToString() + ", " + AppData.pROM()[1].ToString() + ", " + "10" + "," + res.ToString() + "," + AppData.isflalccidControl.ToString());
        //    Debug.Log(_fname);
        //}
        //SceneManager.LoadScene("gameSelection");
    }
    public void SelectpROM()
    {
        promButton.Select();
        aromImage.color = new Color(220f / 255f, 83f / 255f, 87f / 255f, 1f);
        promHandler.isSelected = true;
        aromHandler.isSelected = false;
        SetActiveStatus(aromSelected, false);
        SetActiveStatus(promSelected, true);


    }

    public void SelectAROM()
    {
        promImage.color = new Color(220f / 255f, 83f / 255f, 87f / 255f, 1f);

        aromImage.color =  new Color(0f / 255f, 55f / 255f, 52f / 255f);

        promHandler.isSelected = false;
        aromHandler.isSelected = true;

        SetActiveStatus(aromSelected, true);
        SetActiveStatus(promSelected, true);

        AppData.newPROM = new MechanismData(AppData.selectedMechanism);

        float newPROM_tmin = AppData.newPROM.tmin;
        float newPROM_tmax = AppData.newPROM.tmax;



        Debug.Log(newPROM_tmin + "ee" + newPROM_tmax + "max");

        updateAROM();

    }

    public void updateAROM()
    {
        if (aromHandler.isSelected == true)
        {
            AppData.newPROM = new MechanismData(AppData.selectedMechanism);

            float newPROM_tmin = AppData.newPROM.tmin;
            float newPROM_tmax = AppData.newPROM.tmax;



            Debug.Log(newPROM_tmin + "ee" + newPROM_tmax + "max");
        }
    }
    private void SetActiveStatus(GameObject[] objects, bool status)
    {
       
        foreach (GameObject obj in objects)
        {
            
            obj.SetActive(status);
        }
    }
    //  public void UpdateAssistProfile()
    //{
    //    float assist = 0;

    //    switch (assistTG.ActiveToggles().FirstOrDefault().gameObject.name)
    //    {
    //        case "LOW":
    //            assist = 0.2f;
    //            break;
    //        case "MEDIUM":
    //            assist = 0.6f;
    //            break;
    //        case "HIGH":
    //            assist = 1f;
    //            break;
    //        default:
    //            Debug.LogWarning("Unknown toggle selected");
    //            break;
    //    }

    //    for (int i = 0; i < assistProfile.Length; i++)
    //    {
    //        assistProfile[i] = assist;
    //    }

    //    Debug.Log("Assist array updated");


    //}
    public void OnRedoPRomClicked()
    {
        // Restart Prom and select the Prom panel
        SelectpROM();
        // Optionally, you can reset other states here if needed

        // Optionally, call Start to reinitialize if necessary
        Start();
    }

}
