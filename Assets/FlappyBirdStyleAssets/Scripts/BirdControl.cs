using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BirdControl : MonoBehaviour
{
    private bool isDead = false;
    public static Rigidbody2D rb2d;
    Animator anime;
    // player controls

    public static float playSize;
    public static int FlipAngle = 1;
    public static float tempRobot, tempBird;
    public bool set = false;


    int totalLife = 5;
    int currentLife = 0;
    bool columnHit;
    public Image life;

    public float spriteBlinkingTimer = 0.0f;
    public float spriteBlinkingMiniDuration = 0.1f;
    public float spriteBlinkingTotalTimer = 0.0f;
    public float spriteBlinkingTotalDuration = 2f;
    public bool startBlinking = false;

    float startTime;
    float endTime;

    float targetAngle;

    public FlappyGameControl FGC;
    void Start()
    {
        startTime = 0;
        endTime = 0;
        currentLife = 0;
        anime = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        //anime.SetTrigger("Flap");
        //playSize = Camera.main.orthographicSize;

        playSize = 2.3f + 5.5f;




    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (startTime < 2)
        {
            startTime += Time.deltaTime;
        }
        if (startBlinking == true)
        {
            SpriteBlinkingEffect();

        }

        //Debug.Log(AppData.plutoData.angle);
        if (!isDead && !FGC.gameOver)
        {//
         // 
            if (columnHit)
            {
                anime.SetTrigger("Idle");
                columnHit = false;
            }
            targetAngle = approxRollingAverage(targetAngle, Angle2Screen(PlutoComm.angle));
            transform.position = new Vector2(Mathf.SmoothStep(-13, -7, startTime / 2), Mathf.Clamp(targetAngle, -2.5f, 7));
            //rb2d./*MovePosition*/(new Vector2(-7, Angle2Screen(AppData.plutoData.angle)));
        }
        else if (FGC.gameOver)
        {
            endTime += Time.deltaTime;

            float y = Mathf.Abs(2 * Mathf.Sin(2 * endTime));
            transform.localPosition = new Vector3(transform.position.x + 3 * Time.deltaTime, transform.position.y, 0);
        }

        anime.updateMode = AnimatorUpdateMode.UnscaledTime;

    }
    float approxRollingAverage(float avg, float new_sample)
    {

        avg = avg * 0.9f + 0.1f * new_sample;

        return avg;
    }
    public void SpriteBlinkingEffect()
    {
        spriteBlinkingTotalTimer += Time.deltaTime;
        if (spriteBlinkingTotalTimer >= spriteBlinkingTotalDuration)
        {
            startBlinking = false;
            spriteBlinkingTotalTimer = 0.0f;
            this.gameObject.GetComponent<SpriteRenderer>().enabled = true;   // according to 
                                                                             //your sprite
            return;
        }

        spriteBlinkingTimer += Time.deltaTime;
        if (spriteBlinkingTimer >= spriteBlinkingMiniDuration)
        {
            spriteBlinkingTimer = 0.0f;
            if (this.gameObject.GetComponent<SpriteRenderer>().enabled == true)
            {
                this.gameObject.GetComponent<SpriteRenderer>().enabled = false;  //make changes
            }
            else
            {
                this.gameObject.GetComponent<SpriteRenderer>().enabled = true;   //make changes
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        Debug.Log("Collion " + collision.gameObject.tag);
        if (collision.gameObject.tag == "TopCollider" || collision.gameObject.tag == "BottomCollider")
        {

            startBlinking = true;
            currentLife++;
            life.fillAmount = ((float)currentLife / totalLife);
            // anime.SetTrigger("Die");
            columnHit = true;
            if (currentLife >= totalLife)
            {

                FlappyGameControl.instance.gameduration = -1;
                FlappyGameControl.instance.gameOver = true;
                //FlappyGameControl.instance.BirdDied();
                anime.SetTrigger("Die");
                isDead = true;
                anime.SetTrigger("Die");
            }

        }

    }
    public static float Angle2Screen(float angle)
    {
        MechanismData mechanismData = new MechanismData(AppData.selectedMechanism);
        float tmin = mechanismData.tmin;
        float tmax = mechanismData.tmax;
        return (-2.3f + (angle - tmin) * (playSize) / (tmax - tmin));



    }


}
