
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class panneselect : MonoBehaviour
{

    public Button promButton;
    public Button aromButton;
    public PromWF_Scn_Hndlr_newUI promHandler;
    //public AromWF_Scn_Hndlr_newUI aromHandler;
    public Image promImage;
    public Image aromImage;
    public GameObject[] aromSelected;
    public GameObject[] promSelected;
    private string mech;
    static int steps = 10;
    public static float[] assistProfile = new float[steps];

    public ToggleGroup assistTG;


    // Start is called before the first frame update
    void Start()
    {
        AppData.selectedMechanism = "HOC";
        SelectpROM();
        mech = AppData.selectedMechanism;
        AppData.initializeStuff();
        UpdateAssistProfile();


    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(assistTG.ActiveToggles().FirstOrDefault().gameObject.name);


    }

    public void SelectAROM()
    {
        promImage.color = Color.black;
        aromImage.color = Color.cyan;
        //promHandler.isSelected = false;
        //aromHandler.isSelected = true;

        SetActiveStatus(aromSelected, true);
        SetActiveStatus(promSelected, false);


    }

    public void writeAssesmentFileAndExit()
    {
        UpdateAssistProfile();
       
        //SceneManager.LoadScene("gameSelection");
    }
    public void SelectpROM()
    {
        promButton.Select();
        promImage.color = Color.cyan;
        aromImage.color = Color.black;
        //promHandler.isSelected = true;
        //aromHandler.isSelected = false;
        SetActiveStatus(aromSelected, false);
        SetActiveStatus(promSelected, true);
    }
    private void SetActiveStatus(GameObject[] objects, bool status)
    {

        foreach (GameObject obj in objects)
        {

            obj.SetActive(status);
        }
    }
    public void UpdateAssistProfile()
    {
        float assist = 0;

        switch (assistTG.ActiveToggles().FirstOrDefault().gameObject.name)
        {
            case "LOW":
                assist = 0.2f;
                break;
            case "MEDIUM":
                assist = 0.6f;
                break;
            case "HIGH":
                assist = 1f;
                break;
            default:
                Debug.LogWarning("Unknown toggle selected");
                break;
        }

        for (int i = 0; i < assistProfile.Length; i++)
        {
            assistProfile[i] = assist;
        }

        Debug.Log("Assist array updated");


    }

}
