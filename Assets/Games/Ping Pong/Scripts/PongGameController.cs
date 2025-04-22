using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEditor.SceneManagement;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class PongGameController : MonoBehaviour
{
    public PongGameController Instance {  get; private set; }
    GameObject[] pauseObjects, finishObjects;
    public BoundController rightBound;
    public BoundController leftBound;
    public GameObject ball;
    public Text pointCounter, gameOverText;
    public bool isFinished;
    public bool playerWon, enemyWon;
    public AudioClip[] audioClips; 
    public int enemyScore, playerScore;
    public Vector2 targetPosition;
    private bool isPaused = true;
    private int winningScore = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null)
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
        targetPosition = new Vector2(5.95f, 0f);
        hideFinished();
        GameObject ballClone;
        ballClone = Instantiate(ball, this.transform.position, this.transform.rotation) as GameObject;
        ballClone.transform.SetParent(this.transform);

    }
    void Update()
    {
        pointCounter.text = enemyScore + "\t\t" +
            playerScore;

        //Ball Spawn
        if (transform.childCount == 0)
        {
            GameObject ballClone;
            ballClone = Instantiate(ball, this.transform.position, this.transform.rotation) as GameObject;
            ballClone.transform.SetParent(this.transform);
            EnemyController.stopWatch = 0;
        }
        
        CheckGameEndConditions();

        if (isFinished)
        {
            showFinished();
        }
        else
        {
            if ((Time.timeScale == 0) && !isPaused && !isFinished && !(playerWon || enemyWon))
            {
                Time.timeScale = 1;
            }
        }

        if ((Input.GetKeyDown(KeyCode.P) && !isFinished))
        {
            if (!isPaused)
            { 
                pauseGame();
            }
            else
            {
                resumeGame();
            } 
        }

    }

    private void CheckGameEndConditions()
    {
        if (enemyScore >= winningScore && !isFinished)
        {
            isFinished = true;
            enemyWon = true;
            playerWon = false;
            gameEnd();
        }
        else if (playerScore >= winningScore && !isFinished)
        {
            isFinished = true;
            enemyWon = false;
            playerWon = true;
            gameEnd();
        }
    }

    private void gameEnd()
    {
        Camera.main.GetComponent<AudioSource>().Stop();
        playAudio(enemyWon ? 1 : 0);
        showFinished();
        Time.timeScale = 0;
    }

 private void pauseGame()
    {
        Time.timeScale = 0;
        isPaused = true;
        showPaused();
    }

    private void resumeGame()
    {
        Time.timeScale = 1;
        isPaused = false;
        hidePaused();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Reload()
    {
        playerScore = enemyScore = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void playAudio(int clipNumber)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = audioClips[clipNumber];
        audio.Play();
    } 

    public void showPaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
        }
    }

    public void hidePaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
        }
    }

    public void showFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(true);
        }

        if (playerWon) gameOverText.text = "GAME OVER!\nPLAYER WON!";
        else if (enemyWon) gameOverText.text = "GAME OVER!\nENEMY WON!";
    }

    public void hideFinished()
    {
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(false);
        }
    }

 
}
