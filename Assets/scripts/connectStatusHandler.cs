using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class connectStatusHandler : MonoBehaviour
{
    private Image connectStatus;
    private GameObject loading, textObject;
    private TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        connectStatus = GetComponent<Image>(); // Uncomment if connectStatus is on the same GameObject
        loading = transform.Find("loading").gameObject; // Assuming loading is a child GameObject
                                                        
        textObject = GameObject.Find("statusText");
    }

    void Update()
    {
        // Update connection status
        if (ConnectToRobot.isPLUTO)
        {
            connectStatus.color = Color.green;
            loading.SetActive(false);
            if (textObject != null)
            {
                text = textObject.GetComponent<TextMeshProUGUI>();
                text.text = PlutoComm.frameRate.ToString("F2");
            }
        } 
        else
        {
            connectStatus.color = Color.red;
            loading.SetActive(true);
        }
    }
}
