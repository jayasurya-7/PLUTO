using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using System.IO;

public class HT_spawnTargets1 : MonoBehaviour
{
    public static HT_spawnTargets1 instance;



    //runnnin game 
    public float trailDuration = 3.5f;
    public float stopClock;
    public bool reached;
    public bool onceReached;
    public float reduceOppositeTimer = 0;
    public float playSize = 0;
    private string mech;
    private string hospitalnum;
    public static float[] aRom = { 0, 0 };
    public static float[] pRom = { 0, 0 };
    float prevAng;
    bool angChange;
    public static float targetAngle;
    //GameObject target;
    float toqAmp;
    public int count = 0;
    GameObject target;
    GameObject player;



    bool paramSet;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);

        //Application.targetFrameRate = 300;
        //QualitySettings.vSyncCount = 0;


    }

    // Start is called before the first frame update
    void Start()
    {
        paramSet = false;
        playSize = Camera.main.orthographicSize * Camera.main.aspect;
        setPrameters();
    }
    void Update()
    {
        stopClock -= Time.deltaTime;

        if (!HatGameController.instance.IsPlaying || Time.timeScale == 0 || Mathf.Abs(PlutoComm.angle) > 130)
        {
            stopClock = trailDuration;
        }

    }

    public float Angle2Screen(float angle)
    {
        float newPROM_tmin = AppData.pRomValue[0];
        float newPROM_tmax = AppData.pRomValue[1];

        return Mathf.Lerp(-playSize, playSize, (angle - newPROM_tmin) / (newPROM_tmax - newPROM_tmin));
    }
    public void setPrameters()
    {
        stopClock = trailDuration;
        onceReached = false;
       
        paramSet = true;
    }


   
}




