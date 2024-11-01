using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using System;

public class BoundController : MonoBehaviour
{

    //enemy transform
    public Transform enemy;
    public int enemyScore;
    public int playerScore;
    public AudioClip[] audioClips; // win ,loose

    void Start()
    {
        enemyScore = 0;
        playerScore = 0;
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Target")
        {
            if (other.gameObject.GetComponent<Rigidbody2D>().velocity.x > 0)
            {
                playAudio(1);
                enemyScore++;
                AppData.enemyScore = enemyScore;
                AppData.events = Array.IndexOf(AppData.pongEvents, "playerFail");
                Debug.Log("enemyWINSCORE" + enemyScore);
               
            }
            else
            {
                playerScore++;
                AppData.playerScore = playerScore;
                AppData.gameScore++;
                AppData.events = Array.IndexOf(AppData.pongEvents, "enemyFail");
                playAudio(0);
            }


            //Destroys other object
            Destroy(other.gameObject);

            //sets enemy's position back to original
            enemy.position = new Vector3(-6, 0, 0);
            //pauses game
            if (AppData.enemyScore== AppData.winningScore ||  AppData.playerScore == AppData.winningScore)
            {
                Time.timeScale = 0;
            }
            //Time.timeScale = 0;
        }
    }
    void playAudio(int clipNumber)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = audioClips[clipNumber];
        audio.Play();

    }
}
