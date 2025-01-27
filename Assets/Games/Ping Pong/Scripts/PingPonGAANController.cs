//using PlutoDataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PingPonGAANController : MonoBehaviour
{

    public static PingPonGAANController instance;
     

    // static int trailNumber;
    //public Text trailNUmber;

   
    public float trailDuration = 0f;
    public float playSize = 0;
    public float targetAngle;
    GameObject target;
    GameObject player;

    private float ballTrajetoryPrediction;

    bool wasNonZero;
    BaallTrajectoryPlotter btp;

    public Toggle isFlaccidToggle;
    public bool isFlaccidControlOn;
    bool paramSet = false;
    bool targetSpwan = false;
    private enum DiscreteMovementTrialState { Rest, Moving }
    private DiscreteMovementTrialState trialState = DiscreteMovementTrialState.Rest;
    private DiscreteMovementTrialState _trialState;

    private float targetPosition;
    private float playerPosition;

    public float trialDuration = 0f;
    public float _initialTarget = 0f;
    public float _finalTarget = 0f;
    private void Awake()
    {
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
        playSize = Camera.main.orthographicSize;
        Application.targetFrameRate = 300;

    }



    // Update is called once per frame
    void Update()
    {
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position.y;

        PlutoComm.sendHeartbeat();

        RunTrialStateMachine();
        if (GameObject.FindGameObjectsWithTag("Target").Length > 0)
        {
            btp = GameObject.FindGameObjectWithTag("Target").GetComponent<BaallTrajectoryPlotter>();
            ballTrajetoryPrediction = btp.targetPosition;
            Debug.Log(btp.ballDistance);
            // Debug.Log(btp.ballVelocity.magnitude);
            Debug.Log(PlutoComm.CONTROLTYPE[PlutoComm.controlType]);
            if(btp.transform.position.x < 7.5)
            {
                targetSpwan = true;
            }

            if ((Mathf.Abs(btp.ballDistance) / Mathf.Abs(btp.ballVelocity.magnitude)) < 4 && btp.ballVelocity.x > 0 && (Mathf.Abs(btp.ballDistance) / Mathf.Abs(btp.ballVelocity.magnitude)) > 1)
            {
                if (btp.transform.position.x < 5.5)
                {
                    targetAngle = ScreentoAngle(ballTrajetoryPrediction);
                    targetPosition = ScreentoAngle(ballTrajetoryPrediction);
                    Debug.Log("tg :"+targetAngle);
                }
            }

         
        }




    }
    private void UpdateControlBoundSmoothly()
    {
        if (!targetSpwan) return;
        float t = trialDuration / 2.5f;
        float smoothedControlBound = Mathf.Lerp(0f, 0.5f, t);
        PlutoComm.setControlBound(smoothedControlBound);
        Debug.Log("smoothedControlBound :" + smoothedControlBound);
    }
    private void UpdatePositionTargetSmoothly()
    {
        float t = trialDuration / 3.5f;
        float smoothedTargetPosition = Mathf.Lerp(_initialTarget, _finalTarget, t);
        PlutoComm.setControlTarget(smoothedTargetPosition);
        Debug.Log("smoothedTarget :" + smoothedTargetPosition);
    }
    private void RunTrialStateMachine()
    {
        trialDuration += Time.deltaTime;

        switch (_trialState)
        {
            case DiscreteMovementTrialState.Rest:
                if (targetSpwan && trialDuration >= 0.15f)
                {
                    SetTrialState(DiscreteMovementTrialState.Moving);
                }
                break;

            case DiscreteMovementTrialState.Moving:
                if (targetSpwan)
                {
                    UpdateControlBoundSmoothly();
                    UpdatePositionTargetSmoothly();

                    if (trialDuration >= 4.5f)
                    {
                        if (_finalTarget == _initialTarget)
                        {
                            Debug.Log("Target reached. Returning to Rest state.");
                        }
                        SetTrialState(DiscreteMovementTrialState.Rest);
                    }
                }
                else
                {
                    Debug.Log("Not executed");
                }

                break;
        }
    }
    private void SetTrialState(DiscreteMovementTrialState newState)
    {
        _trialState = newState;

        switch (newState)
        {
            case DiscreteMovementTrialState.Rest:
                trialDuration = 0f;
                targetSpwan = false;
                break;

            case DiscreteMovementTrialState.Moving:
                trialDuration = 0f;
                _initialTarget = PlutoComm.angle;
                _finalTarget = targetAngle;
                PlutoComm.setControlDir((sbyte)(targetPosition > playerPosition ? 1 : -1));

                break;
        }
    }
    public float ScreentoAngle(float y_pos)
    {


        float calibAngleRange = PlutoComm.CALIBANGLE[PlutoComm.mechanism];
        float angle = Mathf.Lerp(
            -calibAngleRange / 2,
            calibAngleRange / 2,
            (y_pos + playSize) / (2 * playSize)
        );
        return angle;


    }
    float getDirection()
    {
        return Mathf.Sign(targetAngle - PlutoComm.angle);
    }




    public static float Angle2Screen(float angle)
    {
        float playSize = 5;
        MechanismData mechanismData = new MechanismData(AppData.selectedMechanism);
        float tmin = mechanismData.tmin;
        float tmax = mechanismData.tmax;
        return Mathf.Clamp(-playSize + (angle - tmin) * (2 * playSize) / (tmax - tmin), -100, 100);

    }


    private void OnApplicationQuit()
    {
        // make 


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
    //    float time;
    //    Debug.Log("amp" + amp);
    //    if (stopClock == trailDuration)
    //    {
    //        time = 0;
    //    }
    //    else
    //    {
    //        time = (trailDuration - stopClock);
    //        time = (time / trailDuration);
    //    }

    //    if (amp != 0)
    //    {
    //        if (Mathf.Abs(targetAngle - AppData.plutoData.angle) > 2 && initialDirection == getDirection())
    //        {

    //            reduceOppositeTimer = 0;

    //            prevTorq = Mathf.SmoothStep(initialTorque, amp, Mathf.Clamp(time, 0, trailDuration));
    //            //Debug.Log("here" + prevTorq);

    //        }
    //        else
    //        {
    //            onceReached = true;
    //            // Debug.Log("Decreasing");

    //            if (Mathf.Abs(targetAngle - PlutoComm.angle) > 3 && initialDirection != getDirection())
    //            {

    //                reduceOppositeTimer += Time.deltaTime;
    //                reduceOppositeTimer = Mathf.Min(reduceOppositeTimer, 3);
    //                prevTorq = prevTorq - Mathf.Sign(prevTorq) * reduceOppositeTimer * 0.01f;
    //            }


    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("zero");
    //        prevTorq = Mathf.SmoothStep(initialTorque, 0, Mathf.Clamp(time, 0, trailDuration));

    //    }
    //    // Debug.Log("fromfunction" + prevTorq );
    //    if (AppData.plutoData.mechIndex != 2)
    //        return prevTorq;
    //    else
    //        return -prevTorq;

    //    //float time = trailDuration - stopClock;
    //    //time = (time / trailDuration);
    //    //if (AppData.regime == "MINIMAL ASSIST" && amp != 0)
    //    //{
    //    //    if (Mathf.Abs(targetAngle - AppData.plutoData.angle) > 2)
    //    //    {
    //    //        if (getDirection() == initialDirection)
    //    //        {
    //    //            // reduceOppositeTimer = 0;
    //    //            if (onceReached == false)
    //    //            {
    //    //                Debug.Log("starting");
    //    //                prevTorq = Mathf.SmoothStep(initialTorque, getDirection() * Mathf.Abs(amp), Mathf.Clamp(time, 0, trailDuration));
    //    //                if (AppData.plutoData.mechIndex != 2)
    //    //                    return prevTorq;
    //    //                else
    //    //                    return -prevTorq;
    //    //            }
    //    //            else
    //    //            {
    //    //                reduceOppositeTimer += Time.deltaTime;
    //    //                reduceOppositeTimer = Mathf.Min(reduceOppositeTimer, 3);
    //    //                if (Mathf.Abs(prevTorq) > 0.05)
    //    //                    prevTorq = prevTorq + Mathf.Sign(prevTorq) * reduceOppositeTimer * 0.01f;
    //    //                if (AppData.plutoData.mechIndex != 2)
    //    //                    return prevTorq;
    //    //                else
    //    //                    return -prevTorq;

    //    //            }

    //    //        }
    //    //        else
    //    //        {
    //    //            reduceOppositeTimer += Time.deltaTime;
    //    //            onceReached = true;
    //    //            if (Mathf.Abs(prevTorq) > 0.05)
    //    //                prevTorq = prevTorq - Mathf.Sign(prevTorq) * reduceOppositeTimer * 0.01f * Mathf.Abs(targetAngle - AppData.plutoData.angle);
    //    //            if (AppData.plutoData.mechIndex != 2)
    //    //                return prevTorq;
    //    //            else
    //    //                return -prevTorq;
    //    //        }
    //    //    }
    //    //    else
    //    //    {
    //    //        if (AppData.plutoData.mechIndex != 2)
    //    //            return prevTorq;
    //    //        else
    //    //            return -prevTorq;
    //    //    }
    //    //}

    //    //else
    //    //{
    //    //    prevTorq = 0;
    //    //    return prevTorq;
    //    //}
    //}
  
}
