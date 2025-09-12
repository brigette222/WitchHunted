using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalker : MonoBehaviour
{
    public Vector2 patrolInterval = new Vector2(1f, 2f); // Patrol wait range
    public float alertRange = 6f, chaseSpeed = 3f, patrolSpeed = 1.5f; // Ranges & speeds

    private Vector2 currentPosition, size = Vector2.one * 0.8f, lastMoveDir = Vector2.down; // Position, check size, last dir
    private bool isMoving = false, shouldChasePlayer = false, isChasing = false; // Movement states
    private LayerMask obstacleMask, walkableMask; // Layer masks
    private List<Vector2> availableMoves = new List<Vector2>(); // Valid patrol moves
    private List<Node> nodesList = new List<Node>(); // Pathfinding nodes
    private Player player; // Player ref
    private SpriteRenderer sr; // Sprite renderer
    private Coroutine movementRoutine; // Movement loop

    [Header("Idle Sprites")] public Sprite[] idleDown, idleUp, idleLeft, idleRight;
    [Header("Walk Sprites")] public Sprite[] walkDown, walkUp, walkLeft, walkRight;
    [Header("Chase Sprites")] public Sprite[] chaseDown, chaseUp, chaseLeft, chaseRight;

    public float frameRate = 0.1f; // Animation frame speed
    private float animationTimer; // Timer for anim
    private int frameIndex; // Frame index

    void Start()
    {
        currentPosition = transform.position; // Start pos
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player"); // Obstacles
        walkableMask = LayerMask.GetMask("Wall", "Enemy"); // Blockers
        sr = GetComponentInChildren<SpriteRenderer>(); // Cache renderer
        player = FindObjectOfType<Player>(); // Find player
        movementRoutine = StartCoroutine(Movement()); // Start loop
    }

    IEnumerator Movement()
    {
        while (true)
        {
            if (!player) { player = FindObjectOfType<Player>(); yield return null; continue; } // Reacquire player
            if ((PauseManager.Instance && PauseManager.Instance.CurrentPauseType == PauseType.Combat) || DialogueManager.IsDialogueActive) { yield return null; continue; } // Skip if paused
            if (isMoving) { yield return null; continue; } // Skip if moving

            float dist = Vector2.Distance(transform.position, player.transform.position); // Dist to player
            if (dist <= alertRange) // Player in range
            {
                shouldChasePlayer = true;
                if (dist <= 1.1f) { yield return new WaitForSeconds(0.5f); continue; } // Too close
                Vector2 nextPos = FindNextStep(transform.position, player.transform.position); // Pathfind
                if (nextPos != (Vector2)transform.position) { lastMoveDir = (nextPos - (Vector2)transform.position).normalized; currentPosition = nextPos; StartCoroutine(SmoothMove(chaseSpeed)); }
                else StartCoroutine(WaitWhileStuck());
            }
            else { shouldChasePlayer = false; Patrol(); } // Patrol if no player
            yield return null;
        }
    }

    void Patrol()
    {
        availableMoves.Clear(); // Reset moves
        CheckDirection(Vector2.up); CheckDirection(Vector2.right); CheckDirection(Vector2.down); CheckDirection(Vector2.left); // Check dirs
        if (availableMoves.Count > 0) { Vector2 move = availableMoves[Random.Range(0, availableMoves.Count)]; lastMoveDir = move.normalized; currentPosition += move; StartCoroutine(SmoothMove(patrolSpeed)); }
        else StartCoroutine(WaitWhileStuck());
    }

    void CheckDirection(Vector2 dir) { if (!Physics2D.OverlapBox(currentPosition + dir, size, 0, obstacleMask)) availableMoves.Add(dir); } // Free dir?

    IEnumerator SmoothMove(float speed)
    {
        isMoving = true; isChasing = shouldChasePlayer; // State
        Sprite[] frames = GetCurrentWalkFrames(); frameIndex = 0; animationTimer = 0f; // Anim setup
        while (Vector2.Distance(transform.position, currentPosition) > 0.01f)
        {
            if ((PauseManager.Instance && PauseManager.Instance.CurrentPauseType == PauseType.Combat) || DialogueManager.IsDialogueActive) { yield return null; continue; } // Pause check
            transform.position = Vector2.MoveTowards(transform.position, currentPosition, speed * Time.deltaTime); // Move
            if (frames != null && frames.Length > 0) { animationTimer += Time.deltaTime; if (animationTimer >= frameRate) { frameIndex = (frameIndex + 1) % frames.Length; sr.sprite = frames[frameIndex]; animationTimer = 0f; } } // Animate
            yield return null;
        }
        transform.position = currentPosition; sr.sprite = GetIdleSprite(); isMoving = false; // Snap idle
        if (!shouldChasePlayer) yield return new WaitForSeconds(Random.Range(patrolInterval.x, patrolInterval.y)); // Patrol delay
    }

    IEnumerator WaitWhileStuck()
    {
        isMoving = true; sr.sprite = GetIdleSprite(); float waitTime = 0.5f, elapsed = 0f;
        while (elapsed < waitTime) { if ((PauseManager.Instance && PauseManager.Instance.CurrentPauseType == PauseType.Combat) || DialogueManager.IsDialogueActive) { yield return null; continue; } elapsed += Time.deltaTime; yield return null; }
        isMoving = false;
    }

    Sprite[] GetCurrentWalkFrames()
    {
        if (isChasing)
        {
            if (lastMoveDir.x > 0 && chaseRight.Length > 0) return chaseRight;
            if (lastMoveDir.x < 0 && chaseLeft.Length > 0) return chaseLeft;
            if (lastMoveDir.y > 0 && chaseUp.Length > 0) return chaseUp;
            if (lastMoveDir.y < 0 && chaseDown.Length > 0) return chaseDown;
        }
        else
        {
            if (lastMoveDir.x > 0 && walkRight.Length > 0) return walkRight;
            if (lastMoveDir.x < 0 && walkLeft.Length > 0) return walkLeft;
            if (lastMoveDir.y > 0 && walkUp.Length > 0) return walkUp;
            if (lastMoveDir.y < 0 && walkDown.Length > 0) return walkDown;
        }
        return null;
    }

    Sprite GetIdleSprite()
    {
        if (lastMoveDir.x > 0 && idleRight.Length > 0) return idleRight[0];
        if (lastMoveDir.x < 0 && idleLeft.Length > 0) return idleLeft[0];
        if (lastMoveDir.y > 0 && idleUp.Length > 0) return idleUp[0];
        if (lastMoveDir.y < 0 && idleDown.Length > 0) return idleDown[0];
        return sr.sprite;
    }

    Vector2 FindNextStep(Vector2 startPos, Vector2 targetPos)
    {
        nodesList.Clear(); nodesList.Add(new Node(startPos, startPos)); int i = 0; Vector2 pos = startPos;
        while (pos != targetPos && i < 1000 && nodesList.Count > 0) { CheckNode(pos + Vector2.up, pos); CheckNode(pos + Vector2.right, pos); CheckNode(pos + Vector2.down, pos); CheckNode(pos + Vector2.left, pos); i++; if (i < nodesList.Count) pos = nodesList[i].position; else break; }
        if (pos == targetPos) { nodesList.Reverse(); for (int j = 0; j < nodesList.Count; j++) { if (pos == nodesList[j].position) { if (nodesList[j].parent == startPos) return pos; pos = nodesList[j].parent; } } }
        return startPos;
    }

    void CheckNode(Vector2 checkpoint, Vector2 parent) { if (!Physics2D.OverlapBox(checkpoint, new Vector2(0.5f, 0.5f), 0, walkableMask)) nodesList.Add(new Node(checkpoint, parent)); }

    public void ResetEnemy() { StopAllCoroutines(); isMoving = shouldChasePlayer = isChasing = false; currentPosition = transform.position; movementRoutine = StartCoroutine(Movement()); }

    public void SetupAnimationSprites(Sprite[] walkDown, Sprite[] walkUp, Sprite[] walkLeft, Sprite[] walkRight, Sprite[] idleDown, Sprite[] idleUp, Sprite[] idleLeft, Sprite[] idleRight, Sprite[] chaseDown, Sprite[] chaseUp, Sprite[] chaseLeft, Sprite[] chaseRight)
    {
        this.walkDown = walkDown; this.walkUp = walkUp; this.walkLeft = walkLeft; this.walkRight = walkRight;
        this.idleDown = idleDown; this.idleUp = idleUp; this.idleLeft = idleLeft; this.idleRight = idleRight;
        this.chaseDown = chaseDown; this.chaseUp = chaseUp; this.chaseLeft = chaseLeft; this.chaseRight = chaseRight;
    }
}

public class Node { public Vector2 position, parent; public Node(Vector2 pos, Vector2 par) { position = pos; parent = par; } }