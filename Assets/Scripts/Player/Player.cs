using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class Player : MonoBehaviour
{
    public static Player instance;

    public float speed;

    LayerMask obstacleMask;
    Vector2 targetPos;
    Transform GFX;
    float flipX;
    bool isMoving;

    private SpriteRenderer sr;

    [Header("Idle Sprites")]
    public Sprite[] idleDown, idleUp, idleLeft, idleRight;

    [Header("Walk Sprites")]
    public Sprite[] walkDown, walkUp, walkLeft, walkRight;

    private float animationTimer;
    public float frameRate = 0.1f;
    private int frameIndex;
    private Vector2 lastMoveDir = Vector2.down;

    [Header("Dialogue Trigger Detection")]
    public float triggerRadius = 1.5f;
    public LayerMask triggerLayer;

    [HideInInspector] public bool canMove = true;

    [Header("Footstep Sound")]
    public AudioSource footstepSource;
    public AudioClip footstepClip;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        transform.position = Vector2.zero;
        targetPos = transform.position;
    }

    void Start()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
        GFX = GetComponentInChildren<SpriteRenderer>().transform;
        sr = GFX.GetComponent<SpriteRenderer>();
        flipX = GFX.localScale.x;

        DialogueRunner runner = FindObjectOfType<DialogueRunner>();
        if (runner != null)
        {
            runner.onDialogueStart.AddListener(() => canMove = false);
            runner.onDialogueComplete.AddListener(() => canMove = true);
        }

        targetPos = transform.position;
    }

    void Update()
    {
        // Don't allow movement if disabled by dialogue or book UI
        if (!canMove || BookUIController.BookIsOpen)
            return;

        Move();
        Animate();
        DetectDialogueTriggers();
        UpdateSortingOrder();
    }

    void Move()
    {
        float horz = System.Math.Sign(Input.GetAxisRaw("Horizontal"));
        float vert = System.Math.Sign(Input.GetAxisRaw("Vertical"));

        Vector2 inputDir = new Vector2(horz, vert);

        if ((Mathf.Abs(horz) > 0) || (Mathf.Abs(vert) > 0))
        {
            lastMoveDir = inputDir;

            if (Mathf.Abs(horz) > 0)
                GFX.localScale = new Vector2(flipX * horz, GFX.localScale.y);

            if (!isMoving)
            {
                if (Mathf.Abs(horz) > 0)
                    targetPos = new Vector2(transform.position.x + horz, transform.position.y);
                else if (Mathf.Abs(vert) > 0)
                    targetPos = new Vector2(transform.position.x, transform.position.y + vert);

                Vector2 hitSize = Vector2.one * 0.8f;
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, obstacleMask);

                if (!hit)
                    StartCoroutine(SmoothMove());
            }
        }
    }

    IEnumerator SmoothMove()
    {
        isMoving = true;
        while (Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;

        PlayFootstep();
    }

    void Animate()
    {
        animationTimer += Time.deltaTime;
        Sprite[] currentFrames = null;

        if (isMoving)
        {
            if (lastMoveDir.x > 0) currentFrames = walkRight;
            else if (lastMoveDir.x < 0) currentFrames = walkLeft;
            else if (lastMoveDir.y > 0) currentFrames = walkUp;
            else if (lastMoveDir.y < 0) currentFrames = walkDown;
        }
        else
        {
            if (lastMoveDir.x > 0) currentFrames = idleRight;
            else if (lastMoveDir.x < 0) currentFrames = idleLeft;
            else if (lastMoveDir.y > 0) currentFrames = idleUp;
            else if (lastMoveDir.y < 0) currentFrames = idleDown;
        }

        if (currentFrames != null && currentFrames.Length > 0)
        {
            if (animationTimer >= frameRate)
            {
                frameIndex = (frameIndex + 1) % currentFrames.Length;
                sr.sprite = currentFrames[frameIndex];
                animationTimer = 0f;
            }
        }
    }

    void UpdateSortingOrder()
    {
        if (sr != null)
        {
            sr.sortingLayerName = "Foreground"; // Make sure this matches the trees
            sr.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
        }
    }

    void DetectDialogueTriggers()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, triggerRadius, triggerLayer);

        if (hit == null) return;

        YarnTriggerZone trigger = hit.GetComponent<YarnTriggerZone>();
        if (trigger != null && !trigger.hasTriggered)
        {
            DialogueRunner runner = FindObjectOfType<DialogueRunner>();
            if (runner != null && !runner.IsDialogueRunning)
            {
                runner.StartDialogue(trigger.dialogueNode);
                trigger.hasTriggered = true;
                hit.gameObject.SetActive(false);
            }
        }
    }

    void PlayFootstep()
    {
        if (footstepSource != null && footstepClip != null)
        {
            footstepSource.PlayOneShot(footstepClip);
        }
    }

    public void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = toggle;
    }
}
