using System.Collections; // For IEnumerator, Coroutine, etc.
using System.Collections.Generic; // For List<T>, Dictionary<T>, etc.
using UnityEngine; // Core Unity types
using UnityEngine.SceneManagement; // For reloading the scene

public enum HallwayWidth { Narrow = 1, Medium = 2, Wide = 3 } // Defines hallway thickness
public enum RoomType { Caverns, Rooms, Winding } // Controls how rooms are generated
public enum RoomShape { Square, Circular } // Determines room shape

public class ForestManager : MonoBehaviour // Main script for procedural forest generation
{
    public GameObject[] randomItems, randomEnemies; // Prefab pools for random items and enemies
    public GameObject FloorPrefab, WallPrefab, tilePrefab, ExitPrefab; // Main tile prefabs for generation
    [UnityEngine.Range(50, 5000)] public int totalFloorCount; // How many tiles to generate
    [UnityEngine.Range(0, 100)] public int itemSpawnPercent; // Chance to spawn items
    [UnityEngine.Range(0, 100)] public int enemySpawnPercent; // Chance to spawn enemies
    public RoomType roomtype; // Controls generation style

    [Header("Item Spawn Blocking")] // Inspector section label
    public LayerMask itemBlockMask; // Prevents item spawns on certain layers (e.g. player, trees)

    [Header("Generation Options")] // Inspector section label
    public bool fixLonelyWalls = true; // If true, converts awkward wall tiles to floor
    public bool spawnEnemies = true; // Enables enemy spawning
    public bool spawnItems = true; // Enables item spawning
    public bool spawnTrees = true; // Enables tree spawning

    [Header("Exit Door Settings")] // Inspector section label
    public bool placeExitDoor = true; // Whether to spawn an exit

    [Header("Walker Behavior")] // Inspector section label
    public bool usePersistentDirection = true; // If true, walker keeps same direction for longer

    [Header("Hallway Settings")] // Inspector section label
    public bool useHallwayWidth = true; // Enables hallway width control
    public HallwayWidth hallwayWidth = HallwayWidth.Narrow; // Width of hallways
    public RoomShape roomShape = RoomShape.Square; // Shape of generated rooms

    [Header("Unique NPCs")] // Inspector section label
    [SerializeField] private GameObject merchantPrefab; // Merchant prefab
    [SerializeField] private GameObject forestNomadPrefab; // Forest nomad prefab
    private Vector3 forestNomadSpawnPosition = Vector3.positiveInfinity; // Used to track spawn location

    [SerializeField] private GameObject organHuskPrefab; // Organ husk prefab
    private Vector3 organHuskSpawnPosition = Vector3.positiveInfinity; // Used to track spawn location

    [SerializeField] private GameObject woundedKnightPrefab; // Wounded knight prefab
    private Vector3 woundedKnightSpawnPosition = Vector3.positiveInfinity; // Used to track spawn location

    public GameObject[] randomTrees; // Pool of random tree prefabs
    [Range(0, 100)] public int treeSpawnPercent; // Chance to spawn trees

    [Header("Sacrificial Altar")] // Inspector section label
    [SerializeField] private GameObject altarPrefab; // Altar prefab
    private Vector3 altarSpawnPosition; // Where the first altar spawns
    [SerializeField] private GameObject secondAltarPrefab; // Optional second altar prefab
    private Vector3 secondAltarSpawnPosition; // Where the second altar spawns

    private Vector3 merchantSpawnPosition = Vector3.positiveInfinity; // Used to track spawn location

    [UnityEngine.Range(0, 100)] public int windingHallPercentage; // For WindingWalker: % chance to make hall vs room
    [HideInInspector] public float minX, maxX, minY, maxY; // Used for boundary clamping and object spawning

    List<Vector3> floorList = new List<Vector3>(); // List of generated floor tile positions
    LayerMask floorMask, wallMask; // Layer masks for physics overlap checks

    [Header("Player Start Settings")] // Inspector section label
    public bool startInRoom = true; // If true, player starts in a room
    public GameObject playerPrefab; // Player prefab to spawn at start

    [Header("Allowed Generation Directions")] // Inspector section label
    public bool allowUp = true; // Allow walker to move up
    public bool allowDown = true; // Allow walker to move down
    public bool allowLeft = true; // Allow walker to move left
    public bool allowRight = true; // Allow walker to move right

    [Header("Generation Rules")] // Inspector section label
    public bool preventLonelyTiles = true; // Prevents isolated floor tiles from being left behind

    private Dictionary<Vector2Int, GameObject> floorTiles = new(); // Grid of placed floor tiles
    private Dictionary<Vector2Int, GameObject> wallTiles = new(); // Grid of placed wall tiles

    public GameObject wallPrefab; // Wall prefab reference (may be redundant with WallPrefab)

    private readonly List<Vector2Int> cardinalDirs = new() // List of 4 cardinal directions
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    void Start() // Unity Start method
    {
        floorMask = LayerMask.GetMask("Floor"); // Cache floor layer mask
        wallMask = LayerMask.GetMask("Wall"); // Cache wall layer mask

        if (roomtype == RoomType.Caverns) // Override settings for caverns
        {
            useHallwayWidth = false; // Disable wide halls
            usePersistentDirection = false; // Force walker to change direction often
        }

        switch (roomtype) // Call correct generation method
        {
            case RoomType.Caverns: RandomWalker(); break; // Use random cavern walker
            case RoomType.Rooms: RoomWalker(); break; // Use room walker
            case RoomType.Winding: WindingWalker(); break; // Use winding walker
        }
    }

    void Update() // Unity Update method (called every frame)
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Backspace)) // In editor, press Backspace to reset scene
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reloads current scene
        }
    }


    void RandomWalker() // Basic random step generator (for caverns)
    {
        startInRoom = false; // Caverns don't start in rooms

        Vector3 curPos = Vector3.zero; // Start at origin

        if (!InFloorList(curPos)) // Check if starting tile is already placed
            floorList.Add(curPos); // Add first tile

        int attempts = 0; // Track number of steps
        int maxAttempts = 100000; // Safety cap to avoid infinite loops

        while (floorList.Count < totalFloorCount && attempts < maxAttempts) // Loop until target floor count
        {
            Vector3 walkDir = RandomDirection(); // Choose a direction
            curPos += walkDir; // Move walker

            if (!InFloorList(curPos)) // Check if this tile is new
                floorList.Add(curPos); // Add tile

            attempts++; // Count this step
        }

        StartCoroutine(DelayProgress()); // Proceed to post-gen phase
    }

    void RoomWalker() // Room generation + connecting hallways
    {
        Vector3 curPos = Vector3.zero; // Start at origin

        if (startInRoom) // Optionally begin with a room
        {
            RandomRoom(curPos); // Spawn starting room
        }
        else
        {
            if (!InFloorList(curPos)) // Add tile if not placed yet
                floorList.Add(curPos);
        }

        while (floorList.Count < totalFloorCount) // Loop until enough floor
        {
            curPos = TakeAHike(curPos); // Move walker in hallway
            RandomRoom(curPos); // Add a room
        }

        StartCoroutine(DelayProgress()); // Begin post-gen logic
    }

    void WindingWalker() // Random hallways, sometimes rooms
    {
        Vector3 curPos = Vector3.zero; // Start at origin

        if (startInRoom) // Optionally start in a room
        {
            RandomRoom(curPos);
        }
        else
        {
            if (!InFloorList(curPos)) // Add single tile if not placed
                floorList.Add(curPos);
        }

        while (floorList.Count < totalFloorCount) // Keep walking until enough tiles
        {
            curPos = TakeAHike(curPos); // Take a hallway step

            int roll = Random.Range(0, 100); // Random number 0–99
            if (roll > windingHallPercentage) // Chance to generate a room
            {
                RandomRoom(curPos); // Add a room
            }
        }

        StartCoroutine(DelayProgress()); // Begin next phase
    }

    Vector3 TakeAHike(Vector3 myPos) // Moves forward a few tiles and places hallways
    {
        Vector3 walkDir = RandomDirection(); // Choose a direction
        int walkLength = Random.Range(9, 18); // Number of steps to take

        List<int> offsets = new(); // Used to widen hallway

        if (!useHallwayWidth) // Single-width hall
        {
            offsets.Add(0);
        }
        else // Wider hallways
        {
            int width = (int)hallwayWidth;

            if (width == 1) offsets.Add(0); // Narrow
            else if (width == 2) offsets.AddRange(new[] { 0, 1 }); // Medium
            else if (width == 3) offsets.AddRange(new[] { -1, 0, 1 }); // Wide
        }

        for (int i = 0; i < walkLength; i++) // Take each step
        {
            if (!usePersistentDirection) // If allowed, reroll direction per step
                walkDir = RandomDirection();

            foreach (int offset in offsets) // For each offset from center
            {
                Vector3 spreadOffset = (walkDir == Vector3.up || walkDir == Vector3.down) // Decide offset axis
                    ? new Vector3(offset, 0, 0)
                    : new Vector3(0, offset, 0);

                Vector3 posToAdd = myPos + spreadOffset; // Tile to place

                if (!InFloorList(posToAdd)) // Avoid duplicates
                    floorList.Add(posToAdd);
            }

            myPos += walkDir; // Move forward
        }

        return myPos; // Return updated position
    }



    void RandomRoom(Vector3 myPos)
    {
        int baseRadius = Random.Range(4, 7); // slightly bigger minimum radius
        float noiseFactor = 0.5f; // how much blobby variation to allow

        for (int x = -baseRadius - 1; x <= baseRadius + 1; x++) // loop over room area X
        {
            for (int y = -baseRadius - 1; y <= baseRadius + 1; y++) // loop over room area Y
            {
                float distance = Mathf.Sqrt(x * x + y * y); // distance from center

                float wobble = Random.Range(-noiseFactor, noiseFactor); // noise for organic shape
                float adjustedRadius = baseRadius + wobble;

                if (roomShape == RoomShape.Circular && distance > adjustedRadius) continue; // shape logic (circular room)

                Vector3 offset = new Vector3(x, y, 0); // position offset from center
                Vector3 pos = myPos + offset; // actual world position

                if (!InFloorList(pos)) floorList.Add(pos); // avoid duplicates
            }
        }
    }

    bool InFloorList(Vector3 myPos)
    {
        for (int i = 0; i < floorList.Count; i++) // check against existing floor tiles
        {
            if (Vector3.Equals(myPos, floorList[i])) return true; // found match
        }
        return false; // no match found
    }

    Vector3 RandomDirection()
    {
        int roll = Random.Range(0, 4);
        switch (roll)
        {
            case 0: return Vector3.up;
            case 1: return Vector3.right;
            case 2: return Vector3.down;
            case 3: return Vector3.left;
            default: return Vector3.up; // fallback
        }
    }




    IEnumerator DelayProgress()
    {
        for (int i = 0; i < floorList.Count; i++) // 1. Place all floor tiles
        {
            Vector3 tilePos = floorList[i]; // Get tile position
            GameObject goTile = Instantiate(tilePrefab, tilePos, Quaternion.identity); // Instantiate floor prefab
            goTile.name = tilePrefab.name; // Name it consistently
            goTile.transform.SetParent(transform); // Parent to this object
        }

        while (FindObjectsOfType<TileSpawner>().Length > 0) yield return null; // 2. Wait until wall spawning is finished

        wallTiles.Clear(); floorTiles.Clear(); // 3. Reset wall/floor tile maps

        foreach (Transform child in transform) // Loop over all spawned tiles
        {
            Vector2Int pos = Vector2Int.RoundToInt(child.position); // Round to grid coords
            if (child.name == "tempWall") // Identify wall tile
            {
                if (!wallTiles.ContainsKey(pos)) wallTiles[pos] = child.gameObject; // Add wall to dictionary
            }
            else if (child.name == tilePrefab.name) // Identify floor tile
            {
                if (!floorTiles.ContainsKey(pos)) floorTiles[pos] = child.gameObject; // Add floor to dictionary
            }
        }

        if (fixLonelyWalls) // 4. Optional cleanup of isolated walls
        {
            FixLonelyWalls(); // Destroy bad wall tiles and replace with floor
            UpdateBitmaskedTileSprites(); // Refresh wall visuals based on surroundings
        }

        if (placeExitDoor) ExitDoorway(); // 5. Optional exit spawn

        if (spawnItems) // 6. Optional item spawns
        {
            Vector2 hitSize = Vector2.one * 0.8f; // Overlap box size for checking neighbors
            for (int x = (int)(minX - 2); x <= (int)(maxX + 2); x++) // Check extended X bounds
            {
                for (int y = (int)(minY - 2); y <= (int)(maxY + 2); y++) // Check extended Y bounds
                {
                    Vector2 checkPos = new Vector2(x, y); // Grid position to check
                    Collider2D hitFloor = Physics2D.OverlapBox(checkPos, hitSize, 0, floorMask); // Look for floor tile
                    if (hitFloor && !Vector2.Equals(hitFloor.transform.position, floorList[floorList.Count - 1])) // Skip if it's the exit tile
                    {
                        Collider2D hitTop = Physics2D.OverlapBox(checkPos + Vector2.up, hitSize, 0, wallMask); // Check wall above
                        Collider2D hitRight = Physics2D.OverlapBox(checkPos + Vector2.right, hitSize, 0, wallMask); // Check wall right
                        Collider2D hitBottom = Physics2D.OverlapBox(checkPos + Vector2.down, hitSize, 0, wallMask); // Check wall below
                        Collider2D hitLeft = Physics2D.OverlapBox(checkPos + Vector2.left, hitSize, 0, wallMask); // Check wall left
                        RandomItems(hitFloor, hitTop, hitRight, hitBottom, hitLeft); // Try to spawn item here
                    }
                }
            }
        }

        if (spawnEnemies) SpawnEnemiesAccurately(); // 7. Optional enemy spawning based on player-safe zone

        SpawnMerchant(); // 8. Always spawn merchant
        SpawnWoundedKnight(); // Always spawn knight
        SpawnAltar(); // Always spawn altar(s)

        if (spawnTrees) RandomTrees(); // 9. Optional tree placement
    }




    void RandomTrees() // Spawns trees only on suitable floor tiles, respecting spacing rules
    {
        Vector2 hitSize = Vector2.one * 0.8f; // Overlap box size for wall detection

        float merchantSafeRadius = 5f; // Min distance from merchant
        float altarSafeRadius = 5f; // Min distance from altar, knight, husk

        foreach (Transform tile in transform) // Iterate through all child tiles
        {
            if (tile.name != "tempFloor") continue; // Skip if not a valid floor tile

            Vector3 pos = tile.position; // Position of this tile

            // Skip if tile is near any blocked character or object
            if (
                Vector3.Distance(pos, merchantSpawnPosition) < merchantSafeRadius ||
                Vector3.Distance(pos, altarSpawnPosition) < altarSafeRadius ||
                Vector3.Distance(pos, secondAltarSpawnPosition) < altarSafeRadius ||
                Vector3.Distance(pos, woundedKnightSpawnPosition) < altarSafeRadius ||
                Vector3.Distance(pos, organHuskSpawnPosition) < altarSafeRadius
            ) continue;

            // Skip if tile is adjacent to any wall
            if (
                Physics2D.OverlapBox(pos + Vector3.up, hitSize, 0, wallMask) ||
                Physics2D.OverlapBox(pos + Vector3.down, hitSize, 0, wallMask) ||
                Physics2D.OverlapBox(pos + Vector3.left, hitSize, 0, wallMask) ||
                Physics2D.OverlapBox(pos + Vector3.right, hitSize, 0, wallMask)
            ) continue;

            // Skip if tile has no nearby wall within 2 tiles in any direction
            if (!(
                Physics2D.OverlapBox(pos + Vector3.up * 2, hitSize, 0, wallMask) ||
                Physics2D.OverlapBox(pos + Vector3.down * 2, hitSize, 0, wallMask) ||
                Physics2D.OverlapBox(pos + Vector3.left * 2, hitSize, 0, wallMask) ||
                Physics2D.OverlapBox(pos + Vector3.right * 2, hitSize, 0, wallMask)
            )) continue;

            // Skip based on tree spawn chance
            if (Random.Range(0, 101) > treeSpawnPercent) continue;

            int treeIndex = Random.Range(0, randomTrees.Length); // Choose random tree prefab
            GameObject tree = Instantiate(randomTrees[treeIndex], pos, Quaternion.identity); // Spawn tree
            tree.name = randomTrees[treeIndex].name; // Name it like prefab
            tree.transform.SetParent(tile); // Parent tree to tile
            tree.transform.position += new Vector3(0, 0, -2); // Push back in Z

            SpriteRenderer sr = tree.GetComponentInChildren<SpriteRenderer>(); // Find sprite renderer
            if (sr != null)
            {
                sr.sortingLayerName = "Foreground"; // Render in front of floors
                sr.sortingOrder = 10000 - Mathf.RoundToInt(pos.y * 100); // Depth sort using Y
            }
        }
    }


    void SpawnAltar()
    {
        Debug.Log("[Altar DEBUG] Starting altar spawn..."); // Debug log for tracking altar spawn initiation

        List<Transform> validCenterTiles = new List<Transform>(); // List to collect all center tiles that are valid for altar placement
        Vector2 hitSize = Vector2.one * 0.8f; // Size of the hitbox used for overlap checks

        int radius = 4; // Radius of 4 units in all directions → 8x8 area centered on the tile
        int requiredEdgeHits = radius * 8; // Number of edge tiles expected around the square (4 edges × 2*radius tiles each)

        foreach (Transform tile in transform) // Loop through each child transform (presumably tiles) under this object
        {
            if (tile.name != "tempFloor") continue; // Only consider tiles named "tempFloor" as potential altar centers

            Vector3 pos = tile.position;
            int edgeHits = 0;

            for (int x = -radius; x <= radius; x++) // Horizontal offset within radius
            {
                for (int y = -radius; y <= radius; y++) // Vertical offset within radius
                {
                    bool isEdge = (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius); // Check if this is an edge tile
                    if (!isEdge) continue;

                    Vector2 checkPos = new Vector2(pos.x + x, pos.y + y); // Compute position to check
                    Collider2D hit = Physics2D.OverlapBox(checkPos, hitSize, 0, floorMask); // Check for floor tile at edge position
                    if (hit != null) edgeHits++; // Count edge tile if it exists
                }
            }

            if (edgeHits >= requiredEdgeHits * 0.9f) // Accept tiles with at least 90% of edge tiles filled
            {
                validCenterTiles.Add(tile);
            }
        }

        if (validCenterTiles.Count == 0) return; // Abort if no valid tiles found

        Transform chosenTile = validCenterTiles[Random.Range(0, validCenterTiles.Count)]; // Choose random valid center tile
        altarSpawnPosition = chosenTile.position;

        GameObject altar = Instantiate(altarPrefab, altarSpawnPosition, Quaternion.identity); // Spawn altar prefab at chosen position
        altar.name = altarPrefab.name;
        altar.transform.SetParent(chosenTile); // Parent altar to chosen tile

        SpriteRenderer sr = altar.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Foreground"; // Set sorting layer for visual stacking
            sr.sortingOrder = 10;
        }
    }

    void SpawnWoundedKnight()
    {
        List<Transform> floorTiles = new List<Transform>(); // Collect all floor tiles under this object
        foreach (Transform tile in transform) // Iterate over all child tiles
        {
            if (tile.name.Contains("Tile") || tile.gameObject.layer == LayerMask.NameToLayer("Floor")) // Match by name or floor layer
            {
                floorTiles.Add(tile); // Add eligible tile to list
            }
        }

        List<Transform> validSpots = new List<Transform>(); // Will store tiles eligible for knight spawn
        Vector2 hitSize = Vector2.one * 0.8f; // Overlap box size for neighbor checks
        float minDistanceFromMerchant = 16f; // Minimum distance from merchant to avoid crowding

        foreach (Transform tile in floorTiles) // Check each floor tile for validity
        {
            Vector3 pos = tile.position;

            if (Vector3.Distance(pos, merchantSpawnPosition) < minDistanceFromMerchant) continue; // Skip if too close to merchant

            bool hasWallLeft = Physics2D.OverlapBox(pos + Vector3.left, hitSize, 0, wallMask); // Must have wall to the left
            bool hasFloorLeft = Physics2D.OverlapBox(pos + Vector3.left, hitSize, 0, floorMask); // Should not have floor behind the wall
            bool hasSpaceRight = !Physics2D.OverlapBox(pos + Vector3.right, hitSize, 0, wallMask); // Require space to the right

            if (hasWallLeft && !hasFloorLeft && hasSpaceRight) // Must meet all placement conditions
            {
                validSpots.Add(tile); // Add tile to spawn options
            }
        }

        if (validSpots.Count == 0) return; // Abort if no valid spots found

        Transform chosenTile = validSpots[Random.Range(0, validSpots.Count)]; // Choose a random valid tile

        Vector3 spawnPos = new Vector3( // Snap position to whole numbers and set Z to -1
            Mathf.Round(chosenTile.position.x),
            Mathf.Round(chosenTile.position.y),
            -1f
        );

        GameObject knight = Instantiate(woundedKnightPrefab, spawnPos, Quaternion.identity); // Spawn knight at the chosen position
        knight.name = woundedKnightPrefab.name;
        knight.transform.SetParent(chosenTile); // Parent to tile for organization

        SpriteRenderer sr = knight.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Foreground"; // Ensure knight renders in correct layer
            sr.sortingOrder = 10;
        }

        woundedKnightSpawnPosition = spawnPos; // Save knight's position to avoid tree overlap later
    }

    void SpawnMerchant()
    {
        List<Transform> floorTiles = new List<Transform>(); // Gather all potential floor tiles
        foreach (Transform tile in transform) // Iterate over each child tile
        {
            if (tile.name.Contains("Tile") || tile.gameObject.layer == LayerMask.NameToLayer("Floor")) // Match by name or floor layer
            {
                floorTiles.Add(tile); // Add tile to the list
            }
        }

        List<Transform> validSpots = new List<Transform>(); // Will hold valid merchant spawn positions
        Vector2 hitSize = Vector2.one * 0.8f; // Size of the overlap check box

        float yMidpoint = (minY + maxY) / 2f; // Middle Y of the map
        float yThreshold = Mathf.Lerp(yMidpoint, maxY, 0.5f); // Prefer upper-middle part of map

        foreach (Transform tile in floorTiles) // Check each candidate tile
        {
            Vector3 pos = tile.position;

            bool hasWallLeft = Physics2D.OverlapBox(pos + Vector3.left, hitSize, 0, wallMask); // Require wall on the left
            bool hasFloorLeft = Physics2D.OverlapBox(pos + Vector3.left, hitSize, 0, floorMask); // Reject if there's floor behind the wall
            bool hasSpaceRight = !Physics2D.OverlapBox(pos + Vector3.right, hitSize, 0, wallMask); // Must be space on the right
            bool highEnough = pos.y >= yThreshold; // Must be in upper half of the map

            if (hasWallLeft && !hasFloorLeft && hasSpaceRight && highEnough) // All conditions met
            {
                validSpots.Add(tile); // Add tile to spawn options
            }
        }

        if (validSpots.Count == 0) return; // Abort if no valid spots

        if (merchantPrefab == null) return; // Abort if merchant prefab not assigned

        Transform chosenTile = validSpots[UnityEngine.Random.Range(0, validSpots.Count)]; // Randomly choose a valid tile

        Vector3 spawnPos = new Vector3( // Round position to integers and set depth to -1
            Mathf.Round(chosenTile.position.x),
            Mathf.Round(chosenTile.position.y),
            -1f
        );

        GameObject merchant = Instantiate(merchantPrefab, spawnPos, Quaternion.identity); // Instantiate merchant at chosen position
        merchant.name = merchantPrefab.name;
        merchant.transform.SetParent(chosenTile); // Parent merchant to tile

        SpriteRenderer sr = merchant.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Foreground"; // Set correct rendering layer
            sr.sortingOrder = 10;
        }

        merchantSpawnPosition = spawnPos; // Save position for use elsewhere (e.g. to block tree spawns)
    }



    void SpawnEnemiesAccurately()
    {
        List<Transform> validSpots = new List<Transform>(); // Stores tiles eligible for enemy spawning

        GameObject player = GameObject.FindWithTag("Player"); // Find the player in the scene
        if (player == null) return; // Abort if no player found

        Vector2Int playerPos = Vector2Int.RoundToInt(player.transform.position); // Round player position to tile grid
        int safeRadius = 5; // Tiles within this radius are considered a safe zone (no enemy spawn)

        int skipped = 0; // Count how many tiles were skipped (debug/stat use)
        int checkedSpots = 0; // Total tiles processed

        foreach (Transform tile in transform) // Iterate through each tile under this object
        {
            checkedSpots++;

            Vector2Int tileGridPos = Vector2Int.RoundToInt(tile.position); // Convert position to grid coordinates
            int distance = Mathf.Abs(tileGridPos.x - playerPos.x) + Mathf.Abs(tileGridPos.y - playerPos.y); // Manhattan distance to player

            if (distance <= safeRadius) // Tile too close to player
            {
                skipped++;
                continue; // Skip spawning here
            }

            Collider2D hitFloor = tile.GetComponent<Collider2D>(); // Ensure the tile has a collider (i.e. it's a floor)
            if (hitFloor == null) continue;

            Vector3 pos = tile.position;
            Vector2 hitSize = Vector2.one * 0.8f; // Size used for surrounding checks

            bool top = Physics2D.OverlapBox(pos + Vector3.up, hitSize, 0, wallMask); // Check wall above
            bool right = Physics2D.OverlapBox(pos + Vector3.right, hitSize, 0, wallMask); // Check wall to the right
            bool bottom = Physics2D.OverlapBox(pos + Vector3.down, hitSize, 0, wallMask); // Check wall below
            bool left = Physics2D.OverlapBox(pos + Vector3.left, hitSize, 0, wallMask); // Check wall to the left

            if (!top && !right && !bottom && !left) // Tile must be open on all four sides
            {
                validSpots.Add(tile); // Add tile to valid spawn list
            }
        }

        int spawnCount = Mathf.RoundToInt(validSpots.Count * (enemySpawnPercent / 100f)); // Decide how many enemies to spawn

        for (int i = 0; i < spawnCount && validSpots.Count > 0; i++) // Spawn loop
        {
            int index = Random.Range(0, validSpots.Count); // Pick a random tile
            Transform chosenSpot = validSpots[index];
            validSpots.RemoveAt(index); // Remove to avoid reuse

            int enemyIndex = Random.Range(0, randomEnemies.Length); // Pick a random enemy prefab
            GameObject goEnemy = Instantiate(randomEnemies[enemyIndex], chosenSpot.position, Quaternion.identity); // Spawn the enemy
            goEnemy.name = randomEnemies[enemyIndex].name;
            goEnemy.transform.SetParent(chosenSpot); // Parent to tile
        }
    }

    void RandomItems(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft)
    {
        Vector2 spawnPos = hitFloor.transform.position; // Get the position of the floor tile
        Vector2 checkSize = Vector2.one * 0.8f; // Size of the box used for overlap checks

        Collider2D blocking = Physics2D.OverlapBox(spawnPos, checkSize, 0, itemBlockMask); // Check if something blocks this spot
        if (blocking != null) return; // Abort if space is already occupied (tree, NPC, player, etc.)

        if ((hitTop || hitRight || hitBottom || hitLeft) && // Must be next to at least one wall
            !(hitTop && hitBottom) && // Can't be boxed in vertically
            !(hitLeft && hitRight)) // Can't be boxed in horizontally
        {
            int roll = Random.Range(0, 101); // Roll a number from 0–100
            if (roll <= itemSpawnPercent) // Compare against item spawn chance
            {
                int itemIndex = Random.Range(0, randomItems.Length); // Pick random item
                GameObject goItem = Instantiate(randomItems[itemIndex], spawnPos, Quaternion.identity); // Spawn it
                goItem.name = randomItems[itemIndex].name;
                goItem.transform.SetParent(hitFloor.transform); // Parent to the tile

                SpriteRenderer sr = goItem.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = "Foreground"; // Ensure it renders above floor but behind trees
                    sr.sortingOrder = 40;
                }
            }
        }
    }

    private void FixLonelyWalls()
{
    HashSet<int> badBitmaskIDs = new() { 5, 7, 10, 11, 13, 14, 15 }; // IDs of bitmask patterns that represent isolated/awkward walls
    bool tilesRemoved; // Tracks if any walls were removed in the last loop

    do // Repeat until no more lonely walls are found
    {
        tilesRemoved = false;
        List<Vector2Int> toRemove = new(); // List of wall positions to remove

        foreach (var kvp in wallTiles) // Loop through all wall tiles
        {
            Vector2Int pos = kvp.Key; // Grid position of wall
            GameObject wallObj = kvp.Value; // Wall GameObject at that position
            RoundedWallTile tile = wallObj.GetComponent<RoundedWallTile>(); // Get the wall's bitmask script

            if (tile == null) continue; // Skip if missing the component

            int bitmask = tile.GetActualBitmask(); // Get actual bitmask for wall
            if (badBitmaskIDs.Contains(bitmask)) // If wall is "lonely", mark for removal
            {
                toRemove.Add(pos);
            }
        }

        foreach (Vector2Int pos in toRemove) // Process all tiles marked for removal
        {
            if (wallTiles.TryGetValue(pos, out GameObject wall)) // Confirm the wall still exists
            {
                Destroy(wall); // Remove the GameObject
                wallTiles.Remove(pos); // Remove from wall dictionary
                tilesRemoved = true; // Track that we made a change
            }

            if (!floorTiles.ContainsKey(pos)) // If there's no floor beneath, create one
            {
                GameObject newFloor = Instantiate(tilePrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, transform); // Instantiate floor
                newFloor.name = tilePrefab.name; // Name it consistently
                floorTiles[pos] = newFloor; // Register it in floor dictionary
            }
        }
    } while (tilesRemoved); // Keep looping if more tiles were removed
}

    private void UpdateBitmaskedTileSprites()
    {
        foreach (var wall in wallTiles.Values) // Loop through all wall tiles
        {
            if (wall == null) continue; // Skip if reference is missing

            RoundedWallTile tile = wall.GetComponent<RoundedWallTile>(); // Get the RoundedWallTile script
            if (tile != null)
            {
                tile.RefreshBitmaskVisual(); // Update the wall’s sprite to match surroundings
            }
        }
    }

    void ExitDoorway()
    {
        Vector3 doorPos = floorList[floorList.Count - 1]; // Get the position of the last floor tile (presumed exit spot)
        Collider2D existingTile = Physics2D.OverlapPoint(doorPos); // Check if something's already at that position
        if (existingTile != null)
        {
            Destroy(existingTile.gameObject); // Remove it to make space for the exit
        }

        GameObject goDoor = Instantiate(ExitPrefab, doorPos, Quaternion.identity); // Instantiate exit at the position
        goDoor.name = ExitPrefab.name;
        goDoor.transform.SetParent(transform); // Parent it to keep the hierarchy clean
    }

}