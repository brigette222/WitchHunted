using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class Player : MonoBehaviour
{
    public static Player instance; // Singleton reference

    public float speed; // Movement speed

    LayerMask obstacleMask; // Walls/enemies mask
    Vector2 targetPos; // Position we move toward
    Transform GFX; // Graphics transform
    float flipX; // Flip scale reference
    bool isMoving; // Movement state

    private SpriteRenderer sr; // Cached SpriteRenderer

    [Header("Idle Sprites")]
    public Sprite[] idleDown, idleUp, idleLeft, idleRight; // Idle animations

    [Header("Walk Sprites")]
    public Sprite[] walkDown, walkUp, walkLeft, walkRight; // Walk animations

    private float animationTimer; // Timer for frame switching
    public float frameRate = 0.1f; // Animation speed
    private int frameIndex; // Current frame index
    private Vector2 lastMoveDir = Vector2.down; // Tracks facing direction

    [Header("Dialogue Trigger Detection")]
    public float triggerRadius = 1.5f; // Auto-dialogue radius
    public LayerMask triggerLayer; // Layers containing dialogue triggers

    [HideInInspector] public bool canMove = true; // Whether movement is allowed

    [Header("Footstep Sound")]
    public AudioSource footstepSource; // Audio source for footsteps
    public AudioClip footstepClip; // Footstep sound clip

    void Awake() // Called before Start
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; } // Enforce singleton
        instance = this; // Assign instance
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load event
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) // Reset position when scene loads
    {
        transform.position = Vector2.zero; // Reset to origin
        targetPos = transform.position; // Sync target
    }

    void Start() // Called once
    {
        obstacleMask = LayerMask.GetMask("Wall", "Enemy"); // Detect walls and enemies
        GFX = GetComponentInChildren<SpriteRenderer>().transform; // Get graphics child
        sr = GFX.GetComponent<SpriteRenderer>(); // Cache SpriteRenderer
        flipX = GFX.localScale.x; // Save initial flip scale

        DialogueRunner runner = FindObjectOfType<DialogueRunner>(); // Find DialogueRunner
        if (runner != null) // Subscribe to dialogue events
        {
            runner.onDialogueStart.AddListener(() => canMove = false); // Lock movement
            runner.onDialogueComplete.AddListener(() => canMove = true); // Unlock movement
        }

        targetPos = transform.position; // Initialize position
    }

    void Update() // Called every frame
    {
        if (!canMove || BookUIController.BookIsOpen) return; // Disable during dialogue/book
        Move(); // Handle movement
        Animate(); // Handle animations
        DetectDialogueTriggers(); // Auto-trigger dialogues
        UpdateSortingOrder(); // Adjust sprite sorting
    }

    void Move() // Handles grid-like movement
    {
        float horz = System.Math.Sign(Input.GetAxisRaw("Horizontal")); // Horizontal input
        float vert = System.Math.Sign(Input.GetAxisRaw("Vertical")); // Vertical input
        Vector2 inputDir = new Vector2(horz, vert); // Build input vector

        if (Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0) // If moving
        {
            lastMoveDir = inputDir; // Update facing direction
            if (Mathf.Abs(horz) > 0) GFX.localScale = new Vector2(flipX * horz, GFX.localScale.y); // Flip horizontally

            if (!isMoving) // Only move if not already moving
            {
                if (Mathf.Abs(horz) > 0) targetPos = new Vector2(transform.position.x + horz, transform.position.y);
                else if (Mathf.Abs(vert) > 0) targetPos = new Vector2(transform.position.x, transform.position.y + vert);

                Vector2 hitSize = Vector2.one * 0.8f; // Collision box size
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, obstacleMask); // Check collision
                if (!hit) StartCoroutine(SmoothMove()); // Move if no obstacle
            }
        }
    }

    IEnumerator SmoothMove() // Smooth transition between tiles
    {
        isMoving = true; // Lock movement
        while (Vector2.Distance(transform.position, targetPos) > 0.01f) // Move until close
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime); // Step toward target
            yield return null; // Wait for next frame
        }
        transform.position = targetPos; // Snap final position
        isMoving = false; // Unlock movement
        PlayFootstep(); // Play sound
    }

    void Animate() // Handles walking/idle animations
    {
        animationTimer += Time.deltaTime; // Update timer
        Sprite[] currentFrames = null; // Which sprite set to use

        if (isMoving) // Walking animations
        {
            if (lastMoveDir.x > 0) currentFrames = walkRight;
            else if (lastMoveDir.x < 0) currentFrames = walkLeft;
            else if (lastMoveDir.y > 0) currentFrames = walkUp;
            else if (lastMoveDir.y < 0) currentFrames = walkDown;
        }
        else // Idle animations
        {
            if (lastMoveDir.x > 0) currentFrames = idleRight;
            else if (lastMoveDir.x < 0) currentFrames = idleLeft;
            else if (lastMoveDir.y > 0) currentFrames = idleUp;
            else if (lastMoveDir.y < 0) currentFrames = idleDown;
        }

        if (currentFrames != null && currentFrames.Length > 0 && animationTimer >= frameRate) // Update frame
        {
            frameIndex = (frameIndex + 1) % currentFrames.Length; // Loop frames
            sr.sprite = currentFrames[frameIndex]; // Set sprite
            animationTimer = 0f; // Reset timer
        }
    }

    void UpdateSortingOrder() // Updates sprite sorting order
    {
        if (sr == null) return; // Skip if no renderer
        sr.sortingLayerName = "Foreground"; // Match environment sorting
        sr.sortingOrder = Mathf.RoundToInt(transform.position.y * -100); // Order based on Y position
    }

    void DetectDialogueTriggers() // Detects Yarn dialogue triggers
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, triggerRadius, triggerLayer); // Check triggers
        if (hit == null) return; // Skip if none
        YarnTriggerZone trigger = hit.GetComponent<YarnTriggerZone>(); // Get trigger
        if (trigger != null && !trigger.hasTriggered) // If not triggered before
        {
            DialogueRunner runner = FindObjectOfType<DialogueRunner>(); // Find DialogueRunner
            if (runner != null && !runner.IsDialogueRunning) // If dialogue not running
            {
                runner.StartDialogue(trigger.dialogueNode); // Start dialogue
                trigger.hasTriggered = true; // Mark as triggered
                hit.gameObject.SetActive(false); // Disable trigger
            }
        }
    }

    void PlayFootstep() // Plays a footstep sound
    {
        if (footstepSource != null && footstepClip != null) footstepSource.PlayOneShot(footstepClip);
    }

    public void ToggleCursor(bool toggle) // Locks/unlocks cursor
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked; // Toggle lock
        Cursor.visible = toggle; // Toggle visibility
    }
}
