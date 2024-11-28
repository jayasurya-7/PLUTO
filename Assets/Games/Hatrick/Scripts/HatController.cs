using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatController : MonoBehaviour
{
    public Camera cam;

    public float maxwidth;
    float targetX;
    public static float tempRobot, tempBird;
    public static int FlipAngle = -1;
    public static float playSize;

    public AudioSource gamesound;
    public AudioClip win;
    public AudioClip loose;
    float[] rom;
    private Vector3 previousPlayerPosition;
    private float playerMovementTime = 0f;
    private Coroutine movementCoroutine;
    // Start is called before the first frame update
    void Start()
    {

        if (cam == null)
        {
            cam = Camera.main;
        }
        Vector3 UpperCorner = new Vector3(Screen.width, Screen.height, 0);
        float hatwidth = GameObject.Find("HatFrontSprite").GetComponent<Renderer>().bounds.extents.x;
        Vector3 targetWidth = cam.ScreenToWorldPoint(UpperCorner);
        maxwidth = targetWidth.x - hatwidth;
        playSize = maxwidth * 0.8f;


    }

    // Update is called once per frame physics
    void Update()
    {

        targetX = Angle2Screen(PlutoComm.angle);
        checkPlayerMovement();


        Vector2 targetPosition = new Vector2(targetX, this.transform.position.y);
        gameObject.GetComponent<Rigidbody2D>().MovePosition(targetPosition);


    }
    private void checkPlayerMovement()
    {
        Vector3 currentPlayerPosition = transform.position;
        float playerDistanceMoved = Vector3.Distance(currentPlayerPosition, previousPlayerPosition); // Calculate the distance moved by the player
        if (playerDistanceMoved > 0.001f)
        {
            if (movementCoroutine == null)
            {
                movementCoroutine = StartCoroutine(trackMovementTime());
            }
        }
        else
        {
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
                movementCoroutine = null;
            }
        }
        previousPlayerPosition = currentPlayerPosition;
    }

    private IEnumerator trackMovementTime()
    {
        while (true)
        {
            playerMovementTime += Time.deltaTime;
            gameData.moveTime = playerMovementTime;
            yield return null;
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {

        Debug.Log(collision.gameObject.name);

        if (collision.gameObject.tag == "Target")
        {

            gamesound.clip = win;
            gamesound.Play();
            gameData.events = Array.IndexOf(gameData.hatEvents, "BallCaught");
            Debug.Log(" ball won   : "+ gameData.events);
            gameData.gameScore++;
            Debug.Log(" ball won");
            HT_spawnTargets1.instance.reached = true;
            HatGameController.instance.balldestroyed = true;
            HatGameController.instance.SpawnTarget();
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Target1")
        {
            gamesound.clip = loose;
            gamesound.Play();
            gameData.events = Array.IndexOf(gameData.hatEvents, "BombCaught");
            gameData.gameScore--;
            Debug.Log("points reduced: " + gameData.events);
            HT_spawnTargets1.instance.reached = true;
            HatGameController.instance.balldestroyed = true;
            HatGameController.instance.SpawnTarget();
            Destroy(collision.gameObject);
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        Debug.Log("Collided");


    }

    public float Angle2Screen(float angle)
    {


        return HT_spawnTargets1.instance.Angle2Screen(angle);


    }
}
