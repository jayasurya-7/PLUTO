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

    void Start()
    {
        PlutoComm.setControlType("POSITIONAAN");
        playSize = Camera.main.orthographicSize;
        Application.targetFrameRate = 300;
    }


    void Update()
    {
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position.y;

        //PlutoComm.sendHeartbeat();
        //if (PlutoComm.CONTROLTYPE[PlutoComm.controlType] != "POSITIONAAN")
        //{
        //    PlutoComm.setControlType("POSITIONAAN");
        //}
        RunTrialStateMachine();
        if (GameObject.FindGameObjectsWithTag("Target").Length > 0)
        {
            btp = GameObject.FindGameObjectWithTag("Target").GetComponent<BaallTrajectoryPlotter>();
            ballTrajetoryPrediction = btp.targetPosition;
            Debug.Log(btp.ballDistance);
            // Debug.Log(btp.ballVelocity.magnitude);
            Debug.Log(PlutoComm.CONTROLTYPE[PlutoComm.controlType]);
            Debug.Log("targetSp:" + gameData.targetSpwan);
            //if(btp.transform.position.x <= 10.5)
            //{
            //    targetSpwan = true;
            //}
            if (gameData.targetSpwan)
            {
              float  Duration = Mathf.Abs(btp.ballDistance) / Mathf.Abs(btp.ballVelocity.magnitude);
                Debug.Log("Timeeee :"+Duration);    
            }

            if ((Mathf.Abs(btp.ballDistance) / Mathf.Abs(btp.ballVelocity.magnitude)) < 4 && btp.ballVelocity.x > 0 && (Mathf.Abs(btp.ballDistance) / Mathf.Abs(btp.ballVelocity.magnitude)) > 1)
            {
                if (btp.transform.position.x < 6.5)
                {
                    targetPosition = ScreentoAngle(ballTrajetoryPrediction);
                    targetAngle = ScreenPositionToAngle(ballTrajetoryPrediction);

                    Debug.Log("tg :"+targetAngle);
                }
            }
        }
    }
    private void UpdateControlBoundSmoothly()
    {
        if (!gameData.targetSpwan) return;
        float t = trialDuration / 2f;
        float smoothedControlBound = Mathf.Lerp(0f, 0.5f, t);
        PlutoComm.setControlBound(smoothedControlBound);
        Debug.Log("smoothedControlBound :" + smoothedControlBound);
    }
    private float ScreenPositionToAngle(float screenPosition)
    {
        float calibAngleRange = PlutoComm.CALIBANGLE[PlutoComm.mechanism];
        float angle = Mathf.Lerp(
            -calibAngleRange / 2,
            calibAngleRange / 2,
            (screenPosition + playSize) / (2 * playSize)
        );
        return angle;
    }
    private void UpdatePositionTargetSmoothly()
    {
        float t = trialDuration / 2.5f;
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
                if (gameData.targetSpwan && trialDuration >= 0.05f)
                {
                    SetTrialState(DiscreteMovementTrialState.Moving);
                }
                break;

            case DiscreteMovementTrialState.Moving:
                if (gameData.targetSpwan)
                {
                    UpdateControlBoundSmoothly();
                    UpdatePositionTargetSmoothly();

                    if (trialDuration >= 4f)
                    {
                        if (_finalTarget == _initialTarget)
                        {
                            Debug.Log("Target reached. Returning to Rest state.");
                        }
                        SetTrialState(DiscreteMovementTrialState.Rest);
                        gameData.isBallReached = false;
                        
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
                gameData.targetSpwan = false;
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
        ROM promAng = new ROM(AppData.selectedMechanism);
        float tmin = promAng.promTmin;
        float tmax = promAng.promTmax;
        return Mathf.Clamp(-playSize + (angle - tmin) * (2 * playSize) / (tmax - tmin), -100, 100);

    }
}
