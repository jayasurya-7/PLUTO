using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slidermech : MonoBehaviour
{

    public GameObject SingleSlider;
    // Start is called before the first frame update
    void Start()
    {
        if(PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) !=3)
        {
            SingleSlider.SetActive(true);
        }
        else {
            SingleSlider.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
