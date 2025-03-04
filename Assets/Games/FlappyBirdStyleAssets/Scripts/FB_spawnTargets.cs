using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using System.IO;

public class FB_spawnTargets : MonoBehaviour
{
    public static FB_spawnTargets instance;

    float prevAng;

    int[] successRate;
    float avgSuccessRate;
    bool dontAssistTrial;

    //runnnin game 
    public float trailDuration = 3;
    public float stopClock;
    public bool reached;
    public bool onceReached;

    public float playSize = 0;
    private string mech;
    private string hospitalnum;
    public static float[] aRom = { 0, 0 };
    public static float[] pRom = { 0, 0 };

    public static float targetAngle;
    //GameObject target;

    GameObject target;
    GameObject player;

    float gameduration = 0;
    public static bool stopAssistance = true;
    public float initialDirection = 0;
    Vector2 targetPos;
    public int win;
    int index = 0;
    public float reduceOppositeTimer = 0;
    public float initialTorque;
    public float prevTorq;
    float prevSpawnTime = 0;
    int val;
    bool setZeroTorque;

    float[] First4Targets;

    public Toggle isFlaccidToggle;

    public bool isFlaccidControlOn;

    int targetcount = 0;
    bool paramSet = false;
    private void Awake()
    {
        Resources.UnloadUnusedAssets();
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);

        Application.targetFrameRate = 300;
        QualitySettings.vSyncCount = 0;


    }

    // Start is called before the first frame update
    void Start()
    {
        paramSet = false;
        System.Random rnd = new System.Random();
        First4Targets = First4Targets.OrderBy(x => rnd.Next()).ToArray();
        val = UnityEngine.Random.Range(50, 100);
        targetcount = -1;



        //setPrameters();
        playSize = 2.3f + 5.5f;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {


        prevSpawnTime += Time.deltaTime;

        stopClock -= Time.deltaTime;




        if (Time.timeScale == 0 || FlappyGameControl.instance.gameOver || Mathf.Abs(PlutoComm.angle) > 130 || targetcount < 0)
        {
            prevTorq = 0;
            stopClock = trailDuration;
        }




    }

    public Vector2 TargetSpawn()
    {
        setZeroTorque = false;
        playSize = BirdControl.playSize;
        onceReached = false;
        reached = false;
        reduceOppositeTimer = 0;


        targetPos = new Vector2(0, 0);
        targetcount++;


        
        //Debug.Log( "Target Angle:" + targetAngle);
        targetPos.y = Angle2Screen(targetAngle);


        return targetPos;

    }
    public void UpdateSuccessRate()
    {
        if (isInPROM(targetAngle))
        {

            int val = onceReached || reached ? 1 : 0;
            Debug.Log(val);
            for (int i = 0; i < successRate.Length; i++)
            {
                if (i <= successRate.Length - 2)
                {
                    successRate[i] = successRate[i + 1];
                }
                else
                    successRate[i] = val;

            }

        }
        avgSuccessRate = (float)successRate.Sum() / (float)successRate.Length;
        Debug.Log(avgSuccessRate);
    }
    public bool isInPROM(float angle)
    {


        float newPROM_tmin = -60f;
        float newPROM_tmax = 60f;
        if (angle < newPROM_tmin || angle > newPROM_tmax)
        {
            Debug.Log("prom target");
            return true;
        }
        else
            return false;

    }
    public float RandomAngle()
    {
        float tmin = -60f;
        float tmax = 60f;
        float prevtargetAngle = targetAngle;
        float tempAngle = Random.Range(tmin, tmax);
        while (Mathf.Abs(tempAngle - prevtargetAngle) < Mathf.Abs(tmax - tmin) / 2.5f)
        {
            tempAngle = Random.Range(tmin, tmax);
        }


        return tempAngle;

    }
    public float Angle2Screen(float angle)
    {
        float tmin = -60f;
        float tmax = 60f;

        return (-2f + (angle - tmin) * (playSize) / (tmax - tmin));


    }


    private void OnApplicationQuit()
    {
    }
    float getDirection()
    {
        return Mathf.Sign(targetAngle - PlutoComm.angle);
    }
}