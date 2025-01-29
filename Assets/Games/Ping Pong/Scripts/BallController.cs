using UnityEngine;
using System.Collections;
using System;



public class BallController : MonoBehaviour
{

    //speed of the ball
    public static float speed = 3.0F;

    //the initial direction of the ball
    private Vector2 spawnDir;

    Vector2 preVel;
    //ball's components
    Rigidbody2D rig2D;

    public AudioClip[] audioClips;
    int rand = 1;
    float threshold = 2;
    void Start()
    {
      
        rig2D = this.gameObject.GetComponent<Rigidbody2D>();
        int rand = UnityEngine.Random.Range(1, 5);

        if (rand == 1)
        {
            spawnDir = new Vector2(-1, 1);
        }
        else if (rand == 2)
        {
            spawnDir = new Vector2(-1, 1);
        }
        else if (rand == 3)
        {
            spawnDir = new Vector2(-1, 1);
        }
        else if (rand == 4)
        {
            spawnDir = new Vector2(-1, 1);
        }

        rig2D.velocity = (spawnDir * speed);

    }

    void FixedUpdate()
    {
        preVel = rig2D.velocity;

        if (rig2D.velocity.magnitude > 0.01f)
        {
            gameData.events = Array.IndexOf(gameData.pongEvents, "moving");
        }
    }
    void playAudio(int clipNumber)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = audioClips[clipNumber];
        audio.Play();
    }

    public void initVelocity(Vector2 velocity)
    {
        rig2D.velocity = velocity;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        playAudio(0);
        if (col.gameObject.tag == "Enemy")
        {

            float y = launchAngle(transform.position,
                                col.transform.position,
                                col.collider.bounds.size.y);

            Vector2 d = new Vector2(1, y).normalized;
            initVelocity(d * speed);

            gameData.events = Array.IndexOf(gameData.pongEvents, "enemyHit");
            gameData.targetSpwan = true;
            gameData.isBallReached = true;
        }

        if (col.gameObject.tag == "Player")
        {
            float y = launchAngle(transform.position,
                                col.transform.position,
                                col.collider.bounds.size.y);

            Vector2 d = new Vector2(-1, y).normalized;
            initVelocity(d * speed);
            gameData.events = Array.IndexOf(gameData.pongEvents, "playerHit");
            gameData.targetSpwan = false;
            gameData.isBallReached = false;
            
        }
        if (col.gameObject.name == "BottomBound")
        {
            if (rig2D.velocity.y == 0)
            {
                rig2D.velocity = new Vector2(rig2D.velocity.x, Mathf.Abs(preVel.y));

            }
            gameData.events = Array.IndexOf(gameData.pongEvents, "wallBounce");
        }
        if (col.gameObject.name == "TopBound")
        {
            if (rig2D.velocity.y == 0)
            {
                rig2D.velocity = new Vector2(rig2D.velocity.x, -Mathf.Abs(preVel.y));
                

            }
            gameData.events = Array.IndexOf(gameData.pongEvents, "wallBounce");
        }
    }

    float launchAngle(Vector2 ballPos, Vector2 paddlePos,
                    float paddleHeight)
    {
        return Mathf.Clamp(0.2f * Mathf.Sign(ballPos.y - paddlePos.y) + (ballPos.y - paddlePos.y) / paddleHeight, -2, 2);
    }


}
