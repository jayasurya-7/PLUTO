using UnityEngine;
using System.Collections;

public class PongPlayerController : MonoBehaviour
{
    public float speed = 10;

    static float topBound = 4.5F;
    static float bottomBound = -4.5F;
    public static float playSize;
    public PongGameController PP;

    void Start()
    {
        playSize = Camera.main.orthographicSize;
        Time.timeScale = 0;
        topBound = playSize - this.transform.localScale.y / 4;
        bottomBound = -topBound;
    }
    void Update()
    {
        this.transform.position = new Vector2(this.transform.position.x, movementControl(this.transform.position.y));
    }

    private float movementControl(float targetY)
    {
        if (Input.GetKey(KeyCode.UpArrow) && targetY < 5f)
        {
                return Mathf.Min(targetY + 0.15f, 8f);
        }

        if (Input.GetKey(KeyCode.DownArrow) && targetY > -5f)
        {
                return Mathf.Max(targetY - 0.15f, -8f);
        }

        // No movement
        return targetY;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //PP.targetPosition = new Vector2(6f, Random.Range(-5f, 6f));
    }

}
