using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalker : MonoBehaviour
{
    public Vector2 patrolInterval = new Vector2(1f, 2f);
    public float alertRange = 6f;
    public float chaseSpeed = 3f;
    public float patrolSpeed = 1.5f;

    private Vector2 currentPosition;
    private Vector2 size = Vector2.one * 0.8f;
    private bool isMoving = false;
    private bool shouldChasePlayer = false;

    private LayerMask obstacleMask;
    private LayerMask walkableMask;

    private List<Vector2> availableMoves = new List<Vector2>();
    private List<Node> nodesList = new List<Node>();

    private Player player;

    [Header("Idle Sprites")]
    public Sprite[] idleDown;
    public Sprite[] idleUp;
    public Sprite[] idleLeft;
    public Sprite[] idleRight;

    [Header("Walk Sprites (Patrol)")]
    public Sprite[] walkDown;
    public Sprite[] walkUp;
    public Sprite[] walkLeft;
    public Sprite[] walkRight;

    [Header("Chase Sprites")]
    public Sprite[] chaseDown;
    public Sprite[] chaseUp;
    public Sprite[] chaseLeft;
    public Sprite[] chaseRight;

    public float frameRate = 0.1f;
    private float animationTimer;
    private int frameIndex;

    private SpriteRenderer sr;
    private Vector2 lastMoveDir = Vector2.down;
    private bool isChasing = false;
    private Coroutine movementRoutine;

    void Start()
    {
        currentPosition = transform.position;
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player");
        walkableMask = LayerMask.GetMask("Wall", "Enemy");

        sr = GetComponentInChildren<SpriteRenderer>();
        player = FindObjectOfType<Player>();

        movementRoutine = StartCoroutine(Movement());
    }

    IEnumerator Movement()
    {
        while (true)
        {
            if (player == null)
            {
                player = FindObjectOfType<Player>();
                yield return null;
                continue;
            }

            if ((PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.Combat)
                || DialogueManager.IsDialogueActive)
            {
                yield return null;
                continue;
            }

            if (isMoving)
            {
                yield return null;
                continue;
            }

            float dist = Vector2.Distance(transform.position, player.transform.position);

            if (dist <= alertRange)
            {
                shouldChasePlayer = true;

                if (dist <= 1.1f)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                Vector2 nextPos = FindNextStep(transform.position, player.transform.position);
                Vector2 moveDir = nextPos - (Vector2)transform.position;

                if (nextPos != (Vector2)transform.position)
                {
                    lastMoveDir = moveDir.normalized;
                    currentPosition = nextPos;
                    StartCoroutine(SmoothMove(chaseSpeed));
                }
                else
                {
                    StartCoroutine(WaitWhileStuck());
                }
            }
            else
            {
                shouldChasePlayer = false;
                Patrol();
            }

            yield return null;
        }
    }

    void Patrol()
    {
        availableMoves.Clear();
        CheckDirection(Vector2.up);
        CheckDirection(Vector2.right);
        CheckDirection(Vector2.down);
        CheckDirection(Vector2.left);

        if (availableMoves.Count > 0)
        {
            Vector2 move = availableMoves[Random.Range(0, availableMoves.Count)];
            lastMoveDir = move.normalized;
            currentPosition += move;
            StartCoroutine(SmoothMove(patrolSpeed));
        }
        else
        {
            StartCoroutine(WaitWhileStuck());
        }
    }

    void CheckDirection(Vector2 dir)
    {
        Collider2D hit = Physics2D.OverlapBox(currentPosition + dir, size, 0, obstacleMask);
        if (!hit)
            availableMoves.Add(dir);
    }

    IEnumerator SmoothMove(float speed)
    {
        isMoving = true;
        isChasing = shouldChasePlayer;

        Sprite[] currentFrames = GetCurrentWalkFrames();
        frameIndex = 0;
        animationTimer = 0f;

        while (Vector2.Distance(transform.position, currentPosition) > 0.01f)
        {
            if ((PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.Combat)
                || DialogueManager.IsDialogueActive)
            {
                yield return null;
                continue;
            }

            transform.position = Vector2.MoveTowards(transform.position, currentPosition, speed * Time.deltaTime);

            if (currentFrames != null && currentFrames.Length > 0)
            {
                animationTimer += Time.deltaTime;
                if (animationTimer >= frameRate)
                {
                    frameIndex = (frameIndex + 1) % currentFrames.Length;
                    sr.sprite = currentFrames[frameIndex];
                    animationTimer = 0f;
                }
            }

            yield return null;
        }

        transform.position = currentPosition;
        sr.sprite = GetIdleSprite();
        isMoving = false;

        if (!shouldChasePlayer)
        {
            yield return new WaitForSeconds(Random.Range(patrolInterval.x, patrolInterval.y));
        }
    }

    IEnumerator WaitWhileStuck()
    {
        isMoving = true;
        sr.sprite = GetIdleSprite();

        float waitTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < waitTime)
        {
            if ((PauseManager.Instance != null && PauseManager.Instance.CurrentPauseType == PauseType.Combat)
                || DialogueManager.IsDialogueActive)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

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
        nodesList.Clear();
        nodesList.Add(new Node(startPos, startPos));

        int listIndex = 0;
        Vector2 myPos = startPos;

        while (myPos != targetPos && listIndex < 1000 && nodesList.Count > 0)
        {
            CheckNode(myPos + Vector2.up, myPos);
            CheckNode(myPos + Vector2.right, myPos);
            CheckNode(myPos + Vector2.down, myPos);
            CheckNode(myPos + Vector2.left, myPos);

            listIndex++;
            if (listIndex < nodesList.Count)
                myPos = nodesList[listIndex].position;
            else
                break;
        }

        if (myPos == targetPos)
        {
            nodesList.Reverse();
            for (int i = 0; i < nodesList.Count; i++)
            {
                if (myPos == nodesList[i].position)
                {
                    if (nodesList[i].parent == startPos)
                        return myPos;

                    myPos = nodesList[i].parent;
                }
            }
        }

        return startPos;
    }

    void CheckNode(Vector2 checkpoint, Vector2 parent)
    {
        Collider2D hit = Physics2D.OverlapBox(checkpoint, new Vector2(0.5f, 0.5f), 0, walkableMask);
        if (!hit)
        {
            nodesList.Add(new Node(checkpoint, parent));
        }
    }

    // ? Resumes movement after reload/respawn
    public void ResetEnemy()
    {
        StopAllCoroutines();
        isMoving = false;
        shouldChasePlayer = false;
        isChasing = false;
        currentPosition = transform.position;
        movementRoutine = StartCoroutine(Movement());
    }

    // ? Needed by EnemyVisualSetup to assign sprites
    public void SetupAnimationSprites(
        Sprite[] walkDown, Sprite[] walkUp, Sprite[] walkLeft, Sprite[] walkRight,
        Sprite[] idleDown, Sprite[] idleUp, Sprite[] idleLeft, Sprite[] idleRight,
        Sprite[] chaseDown, Sprite[] chaseUp, Sprite[] chaseLeft, Sprite[] chaseRight)
    {
        this.walkDown = walkDown;
        this.walkUp = walkUp;
        this.walkLeft = walkLeft;
        this.walkRight = walkRight;

        this.idleDown = idleDown;
        this.idleUp = idleUp;
        this.idleLeft = idleLeft;
        this.idleRight = idleRight;

        this.chaseDown = chaseDown;
        this.chaseUp = chaseUp;
        this.chaseLeft = chaseLeft;
        this.chaseRight = chaseRight;
    }
}

public class Node
{
    public Vector2 position;
    public Vector2 parent;

    public Node(Vector2 pos, Vector2 par)
    {
        position = pos;
        parent = par;
    }
}