using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlutoComm.setControlType("NONE");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void game() {
        SceneManager.LoadScene("HatrickGame");
    }

    public void menu() {
        SceneManager.LoadScene("choosegame");
    }
}
