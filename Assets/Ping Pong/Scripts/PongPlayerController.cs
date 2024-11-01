 using UnityEngine;
using System.Collections;

public class PongPlayerController : MonoBehaviour
{

    //speed of player
    public float speed = 10;

    //bounds of player
    static float topBound = 4.5F;
    static float bottomBound = -4.5F;
    // player controls

    public static float playSize;
    public static float[] rom;
    public float ballTrajetoryPrediction;
    public static int reps;

    void Start()
    {
        playSize = Camera.main.orthographicSize;
        AppData.reps = 0;
        Time.timeScale = 0;
        topBound = playSize - this.transform.localScale.y / 4;
        bottomBound = -topBound;
    }



    // Update is called once per frame
    void Update()
    {
        this.transform.position = new Vector2(this.transform.position.x, Angle2Screen(PlutoComm.angle));

    }
    public static float Angle2Screen(float angle)
    {
        MechanismData mechanismData = new MechanismData(AppData.selectMechanism);
        float tmin = mechanismData.tmin;
        float tmax = mechanismData.tmax;
        Debug.Log("tmin_"+ tmin +"tmax_"+tmax);

        return Mathf.Clamp(-playSize + (angle - tmin) * (2 * playSize) / (tmax - tmin), bottomBound, topBound);

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Target")
        {
            AppData.reps += 1;
            Debug.Log(AppData.reps);

        }
    }
    private void OntriggerEnter2D(Collision2D collision)
    {
        Debug.Log("hello");
    }
}
