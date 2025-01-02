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




        //   Debug.Log(onceReached.ToString() + "," + targetAngle +","+ AppData.plutoData.angle);


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
        if (targetcount > 3)
        {
            targetAngle = RandomAngle();
        }
        else
        {

            targetAngle = First4Targets[targetcount];

        }

        dontAssistTrial = false;
        if (isInPROM(targetAngle) && avgSuccessRate >= 0.8)
        {
            dontAssistTrial = true;
        }
        //Debug.Log( "Target Angle:" + targetAngle);
        targetPos.y = Angle2Screen(targetAngle);


        initialDirection = getDirection();


        prevAng = initialDirection;
        initialTorque = prevTorq;
        onceReached = false;
        target = GameObject.FindGameObjectWithTag("Target");
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

        AppData.newPROM = new MechanismData(AppData.selectedMechanism);


        float newPROM_tmin = AppData.newPROM.tmin;
        float newPROM_tmax = AppData.newPROM.tmax;
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
        MechanismData mechanismData = new MechanismData(AppData.selectedMechanism);
        float tmin = mechanismData.tmin;
        float tmax = mechanismData.tmax;
        float prevtargetAngle = targetAngle;
        float tempAngle = Random.Range(tmin,tmax);
        while (Mathf.Abs(tempAngle - prevtargetAngle) < Mathf.Abs(tmax - tmin) / 2.5f)
        {
            tempAngle = Random.Range(tmin, tmax);
        }


        return tempAngle;

    }
    public float Angle2Screen(float angle)
    {
        MechanismData mechanismData = new MechanismData(AppData.selectedMechanism);
        float tmin = mechanismData.tmin;
        float tmax = mechanismData.tmax;

        return (-2f + (angle - tmin) * (playSize) / (tmax - tmin));


    }

    //public void setPrameters()
    //{

    //    mech = AppData.plutoData.mechs[AppData.plutoData.mechIndex];
    //    aRom = AppData.aROM();
    //    pRom = AppData.pROM();

    //    isFlaccidControlOn = false;

    //    checkIfFlaccid();


    //    PlutoDataStructures.AAN aanprofile = new PlutoDataStructures.AAN(AppData.subjHospNum, AppData.plutoData.mechs[AppData.plutoData.mechIndex]);



    //    assistanceTorque = aanprofile.profile;
    //    isFlaccidControlOn = aanprofile.isFlaccid == 1 ? true : false;
    //    isFlaccidToggle.isOn = isFlaccidControlOn;
    //    initialTorque = prevTorq;
    //    stopClock = trailDuration;
    //    onceReached = false;


    //    stepSize = (pRom[1] - pRom[0]) / (steps - 1);

    //    for (int i = 0; i < assistanceAngle.Length; i++)
    //    {
    //        assistanceAngle[i] = pRom[0] + stepSize * i;
    //        if (i == assistanceAngle.Length)
    //        {
    //            assistanceAngle[i] = pRom[1];
    //        }

    //    }


    //    paramSet = true;
    //}

    //void checkIfFlaccid()
    //{
    //    float[] maxROM = { 100, 50, 120, 75, 100, 100 };

    //    if (pRom[0] - pRom[1] < 10)
    //    {
    //        isFlaccidControlOn = true;
    //    }
    //    else
    //        isFlaccidControlOn = false;
    //}

    //public float getTorque(float targetAngle)
    //{

    //    float torque;
    //    targetAngle = 1;/*Mathf.Clamp(targetAngle, AppData.pROM()[0], AppData.pROM()[1]);*/
    //    int i = Array.FindIndex(assistanceAngle, k => targetAngle <= k);
    //    i = i == -1 ? assistanceAngle.Length - 1 : i;

    //    if (i > 0)
    //    {
    //        torque = assistanceTorque[i - 1] + (targetAngle - assistanceAngle[i - 1]) * (assistanceTorque[i] - assistanceTorque[i - 1]) / (assistanceAngle[i] - assistanceAngle[i - 1]);

    //    }
    //    else
    //    {
    //        torque = assistanceTorque[i];
    //    }
    //    Debug.Log(String.Join(",", assistanceAngle));
    //    Debug.Log(String.Join(",", assistanceTorque));
    //    Debug.Log("Index:" + i + "Target:" + targetAngle);
    //    Debug.Log("Index:" + i + "Target:" + torque);

    //    torque = Mathf.Clamp(torque, assistanceTorque.Min(), assistanceTorque.Max());
    //    return (torque);
    //}

    private void OnApplicationQuit()
    {
    }
    float getDirection()
    {
        return Mathf.Sign(targetAngle - PlutoComm.angle);
    }

    //public float TorqueProfile(float amp)
    //{
    //    if (!isFlaccidControlOn)
    //    {
    //        return (normalController(amp));
    //    }
    //    else
    //    {
    //        float assistanceTorque = Mathf.Abs(amp) < 0.2 ? 0.2f : Mathf.Abs(amp);
    //        Debug.Log("flaccid" + assistanceTorque);
    //        return (flaccidController(assistanceTorque));
    //    }

    //}

    //float flaccidController(float amp)
    //{
    //    float time = trailDuration - stopClock;
    //    time = (time / trailDuration);

    //    //Debug.Log(amp);
    //    if (dontAssistTrial)
    //    {
    //        prevTorq = 0;
    //    }
    //    else
    //    {
    //        if (Mathf.Abs(targetAngle - PlutoComm.angle) > 5 && initialDirection == getDirection() && !onceReached)
    //        {
    //            reduceOppositeTimer = 0;
    //            prevTorq = Mathf.SmoothStep(initialTorque, getDirection() * amp, Mathf.Clamp(time, 0, trailDuration));

    //        }
    //        else
    //        {
    //            onceReached = true;

    //            if (Mathf.Abs(targetAngle - PlutoComm.angle) > 3)
    //            {
    //                reduceOppositeTimer += Time.deltaTime;
    //                reduceOppositeTimer = Mathf.Min(reduceOppositeTimer, 3);
    //                if (Mathf.Abs(prevTorq) > 0.05)
    //                    prevTorq = prevTorq + getDirection() * reduceOppositeTimer * 0.01f;
    //            }


    //        }
    //    }
    //    prevTorq = Mathf.Clamp(prevTorq, assistanceTorque.Min(), assistanceTorque.Max());


    //    Debug.Log(prevTorq);
    //}

    //float normalController(float amp)
    //{
    //    float time = trailDuration - stopClock;
    //    time = (time / trailDuration);

    //    Debug.Log(amp);
    //    if (dontAssistTrial)
    //    {
    //        prevTorq = 0;
    //    }
    //    else
    //    {
    //        if (Mathf.Abs(targetAngle - PlutoComm.angle) > 5 && initialDirection == getDirection() && !onceReached)
    //        {
    //            reduceOppositeTimer = 0;
    //            prevTorq = Mathf.SmoothStep(initialTorque, amp, Mathf.Clamp(time, 0, trailDuration));

    //        }
    //        else
    //        {
    //            onceReached = true;

    //            if (Mathf.Abs(targetAngle - PlutoComm.angle) > 3)
    //            {
    //                reduceOppositeTimer += Time.deltaTime;
    //                reduceOppositeTimer = Mathf.Min(reduceOppositeTimer, 3);
    //                if (Mathf.Abs(prevTorq) > 0.05)
    //                    prevTorq = prevTorq + getDirection() * reduceOppositeTimer * 0.01f;
    //            }


    //        }
    //    }
    //    prevTorq = Mathf.Clamp(prevTorq, assistanceTorque.Min(), assistanceTorque.Max());

    //    return prevTorq;
    //}

    //public class AAN
    //{


    //    public bool isInAROM(float angle)
    //    {
    //        if (aRom[0] <= angle && angle <= aRom[1])
    //        {
    //            return true;
    //        }

    //        else
    //        {
    //            return false;
    //        }

    //    }



    //    public float Getindex(float angle)
    //    {
    //        float temp = -999;
    //        temp = (angle - pRom[0]) / stepSize;

    //        return temp;
    //    }

    //    public int GetindexCorrected(float angle)
    //    {
    //        int i = Array.FindIndex(assistanceAngle, k => angle <= k);

    //        return i;

    //    }




    //}
}




