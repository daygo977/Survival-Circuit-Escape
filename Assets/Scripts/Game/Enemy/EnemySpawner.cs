using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Spawn Enemies, either in set numbers, or looped waves:
/// - Can only spawn from spawn locations, and have a radius when player is within radius, spawn enemies.
/// - Never goes pass enemy alive cap, when enemy is defeated another takes its place until wave is complete
/// - Enemies have prevention from stacking against each other
/// 
/// Script is for game object EnemySpawner:
/// - Create children objects below parent (P1, P2, and so on)
/// - Uses enemy prefab
/// 
/// Process:
/// - Start() -> SpawnLoop() coroutine runs -> repeatedly TrySpawnOne() until spawnCount for that wave.
/// Each enemy spawned enemy will get EnemySpawnTicket (class), which calls NotifyEnemyGone() when it dies,
/// so we can spawn a replacement if we're below maxAlive.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;          //Which enemy to spawn (only 1 for the moment)
    public Transform parentForEnemies;      //Parent in hierarchy
    public LayerMask enemyMask;             //Layer that spawned enemies use
    public LayerMask obstacleMask;          //Layer that walls and obstacles use

    [Header("Spawn Points")]
    public Transform[] points;              //Where enemies are allow to appear (spawners)

    [Header("Timing")]
    public float initialDelay = 0.25f;      //Delay before initial enemies spawn
    public float spawnDelay = 0.15f;        //Delay before individual spawns

    [Header("Wave")]
    public int spawnCount = 10;             //Total enemies to spawn in waves (changeable)
    public bool loop = false;               //If true, after wave finishes, wait loop pause then spawn next wave
    public float loopPause = 3f;

    [Header("Collision Clearance")]
    public float clearRadius = 0.5f;        //Check circular area for each enemy so that they don't overlap
    public int maxPlacementTries = 8;       //How many random jitter attempts will be attempted before spot is blocked
    public float jitterRadius = 0.6f;       //How far around the spawn point is allowed to jitter

    [Header("Alive Cap")]
    public int maxAlive = 25;               //Max number of enemies in the current map

    [Header("Activation (player proximity)")]
    public float activationRadius = 10f;    //Spawn point must be within distance of player to be active
    public Transform player;                //Players reference (auto-filled if null)

    int _alive = 0;         //How many enemies that are spawned are currently alive
    int _spawned = 0;       //How many enemies are spawned in so far for wave (resets each loop)

    void OnValidate()
    {
        //If points are null or length 0, grab all children of spawner (parent)
        //so we can create empty children for P1, P2, etc. (spawners)
        if (points == null || points.Length == 0)
        {
            int n = transform.childCount;
            if (n > 0)
            {
                points = new Transform[n];
                for (int i = 0; i < n; i++)
                {
                    points[i] = transform.GetChild(i);
                }
            }
        }
    }

    void Start()
    {
        //Auto-find player by tag if not assigned in Inspector
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p)
            {
                player = p.transform;
            }
        }
        //Start coroutine that manahes waves/loop
        StartCoroutine(SpawnLoop());
    }

    /// Main Wave Logic:
    /// 1. Initial Delay
    /// 2. Reset counters (_alive and _spawned) at the start of each wave
    /// 3. While we have not spawned spawnCount:
    ///     - If we are at or above maxAlive wait
    ///     - Else, TrySpawnOne(). If good, increment _spawned and wait for spawnDelay
    /// 4. If loop is true, pause (loopPause), then start wave logic again.
    IEnumerator SpawnLoop()
    {
        //Wait before first spawn so the scene has time to initialize
        yield return new WaitForSeconds(initialDelay);

        do
        {
            //New wave stats
            _alive = 0;
            _spawned = 0;

            //Keep spawning until we hit spawnCount for enemies in this wave
            while (_spawned < spawnCount)
            {
                //Check alive cap, only spawn if we are under maxAlive
                if (_alive >= maxAlive)
                {
                    //Don't spawn yet, just wait frame
                    yield return null;
                    continue;
                }

                // Try to spawn new enemy
                bool didSpawn = TrySpawnOne();

                //If enemy is spawned, increment _spawned, and do small delay between each spawn
                //Else, could not spawn (either no valid point or clear spots) so wait frame
                //Helps prevent overlapping from quick spawns
                if (didSpawn)
                {

                    _spawned++;
                    yield return new WaitForSeconds(spawnDelay);
                }
                else
                {
                    yield return null;
                }
            }

            //We have spawned this wave and hit spawnCount
            //If loop is true, wait the loop pause, then start new wave iteration
            if (loop)
            {
                yield return new WaitForSeconds(loopPause);
            }
        }
        while (loop);
    }

    /// Attempts to spawn one enemy.
    /// Picks active spawn point near player (if player in radius for spawn point).
    /// Finds non-overlapping position, and instantiates new enemy, and registers a
    /// ticket so we can track _alive.
    /// Returns true if good, False, if we could not find anywhere that meets conditions
    bool TrySpawnOne()
    {
        //Safe checks
        if (!enemyPrefab || points == null || points.Length == 0)
        {
            return false;
        }

        //Pick spawn point that is close enough (within radius, activationRadius) to player
        Transform spawnPoint;
        if (!PickActiveSpawnPoint(out spawnPoint))
        {
            return false;
        }

        //Within area, try to find a final/clear spot that doesn't overlap enemies/walls
        Vector2 finalPos;
        if (!TryFindClearSpot(spawnPoint.position, out finalPos))
        {
            return false;
        }

        //Spawn the enemy, increment _alive
        var go = Instantiate(enemyPrefab, finalPos, Quaternion.identity, parentForEnemies ? parentForEnemies : null);
        _alive++;

        //Add spawn ticket to spawned enemy, so when enemy dies or disables,
        //it can tell back one less enemy and free slot available
        var ticket = go.AddComponent<EnemySpawnTicket>();
        ticket.owner = this;

        //New (10/27/2025)
        //refresh HUD
        UpdateHUDEnemyInfo();

        return true;
    }

    /// Picks a spawn point that is considered active:
    /// - If player exists: spawn point must be within activationRadius of player
    /// - If player doesn't exist, just pick randomly
    /// Also randomizes start index, so it doesn't always choose P1 spawn point
    bool PickActiveSpawnPoint(out Transform chosen)
    {
        chosen = null;

        //Safe check
        if (points == null || points.Length == 0)
        {
            return false;
        }

        //If player doesn't exist, randomize indices so doesn't bias towards P1
        if (!player)
        {
            chosen = points[Random.Range(0, points.Length)];
            return chosen != null;
        }

        //Compare sqr distances, and randomize points so we don't get a points bias (all within activationRadius)
        float radiusSqr = activationRadius * activationRadius;
        int start = Random.Range(0, points.Length);

        //Check each point once, from start and wrapping
        for (int offset = 0; offset < points.Length; offset++)
        {
            int index = (start + offset) % points.Length;
            var pt = points[index];
            if (!pt)
            {
                continue; //skip null/removed points (safely)
            }

            // How far point is from player difference
            Vector2 diff = (Vector2)pt.position - (Vector2)player.position;
            //Is this point close enough to player to be active
            if (diff.sqrMagnitude <= radiusSqr)
            {
                chosen = pt;
                return true;
            }
        }

        return false;
    }

    /// Called by EnemySpawnTicket (class) when one of spawned enemies dies or disables.
    /// This frees up an alive slot so SpawnLoop can replace by spawning another enemy.
    public void NotifyEnemyGone()
    {
        _alive = Mathf.Max(0, _alive - 1);
    }

    /// Trys to pick a non-overlapping position:
    /// - Try first to center exactly
    /// - If blocked, try another place (up to maxPlacementTries) with random jitters within jitterRadius
    /// Returns true and found postion, else false
    bool TryFindClearSpot(Vector2 center, out Vector2 pos)
    {
        //Try center position
        if (IsClear(center))
        {
            pos = center;
            return true;
        }

        //Try random offsets (jitterRadius) around point
        for (int i = 0; i < maxPlacementTries; i++)
        {
            float angle = Random.value * Mathf.PI * 2f;
            float r = Random.Range(jitterRadius * 0.3f, jitterRadius);
            Vector2 cand = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
            if (IsClear(cand))
            {
                pos = cand;
                return true;
            }
        }

        //No point found, use center but return false
        pos = center;
        return false;
    }

    /// Checks if circle at point p is clear
    /// - No overlap with other enemies (enemyMask)
    /// - No overlap with obstacles/walls (obstacleMask)
    bool IsClear(Vector2 p)
    {
        // Enemy here already?
        if (Physics2D.OverlapCircle(p, clearRadius, enemyMask))
        {
            return false;
        }

        // Blocked by wall/obstacle already?
        if (obstacleMask.value != 0 && Physics2D.OverlapCircle(p, clearRadius, obstacleMask))
        {
            return false;
        }
        return true;
    }

    /// Added (10/27/2025)
    /// Pushes enemy info (enemies currently alive and enemies left in wave)
    /// Helper function
    void UpdateHUDEnemyInfo()
    {
        int remainToSpawn = Mathf.Max(0, spawnCount - _spawned);
        int waveLeftTotal = _alive + remainToSpawn;
        HUDManager.SetEnemyInfo(_alive, waveLeftTotal);
    }

    /// Scene/Editor debug gizmos:
    /// - Green = clearRadius (must be empty to spawn there)
    /// - Magenta = jitterRadius (random offset search area)
    /// - Cyan = activiationRadius (how close player must be to be active)
    /// Shows around each spawnpoint
    void OnDrawGizmos()
    {
        if (points != null)
        {
            foreach (var t in points)
            {
                if (!t)
                {
                    continue;
                }

                //Small circle
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(t.position, clearRadius);

                //Mid circle
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(t.position, jitterRadius);

                //Large circle
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(t.position, activationRadius);
            }
        }
    }
}
