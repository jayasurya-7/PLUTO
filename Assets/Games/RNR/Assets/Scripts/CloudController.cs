using UnityEngine;

public class CloudController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public SpriteRenderer cloudRenderer;   
    public Sprite[] emotionSprites;        
    public float emotionChangeInterval = 5f, PLAYSIZE; 
    private float emotionTimer = 0f, position;
    private float[] aprom;
    public ParticleSystem rainEffect;
    public Collider2D rainAreaCollider;   // <- assign RainArea BoxCollider2D in Inspector
    private bool isRaining = false;
    void Start()
    {
        ChangeEmotion();
        aprom = AppData.Instance.selectedMechanism.CurrentAProm;

        PLAYSIZE = Camera.main.orthographicSize * Camera.main.aspect;


        // Optional auto-find if you forgot to assign:
        if (rainAreaCollider == null)
        {
            var child = transform.Find("RainArea");
            if (child != null) rainAreaCollider = child.GetComponent<Collider2D>();
        }

        if (rainAreaCollider != null) rainAreaCollider.enabled = false; // start off
    }

    void Update()
    {
        position = AngleToScreen(PlutoComm.angle);

        // position = (AppData.Instance.IsTrainingSide("RIGHT") && AppData.Instance.selectedMechanism.IsMechanism("HOC")) ? HatGameController.Instance.AngleToScreen(-PlutoComm.angle):HatGameController.Instance.AngleToScreen((PlutoComm.angle));
        // MovementTracker.UpdatePosition( this.transform.position);

        Vector2 pos = new Vector2(position, this.transform.position.y);
        // Movement
//         float moveX = Input.GetAxis("Horizontal");
// transform.position += new Vector3(moveX, 0, 0) * moveSpeed * Time.deltaTime;

// Clamp to camera view
// Vector3 pos = transform.position;
float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
float leftBound = -halfWidth + 0.5f;   // margin
float rightBound = halfWidth - 0.5f;
pos.x = Mathf.Clamp(pos.x, leftBound, rightBound);
transform.position = pos;

        // Emotion change
        emotionTimer += Time.deltaTime;
        if (emotionTimer >= emotionChangeInterval)
        {
            ChangeEmotion();
            emotionTimer = 0f;
        }

       
    }

    public void ChangeEmotion()
    {
        if (emotionSprites.Length > 0)
        {
            int index = Random.Range(0, emotionSprites.Length);
            cloudRenderer.sprite = emotionSprites[index];
        }
    }
   public float AngleToScreen(float angle) => Mathf.Lerp(-PLAYSIZE, PLAYSIZE, (angle - aprom[0]) / (aprom[1]- aprom[0]));

}
