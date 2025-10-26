using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;
    public Transform parentForEnemies;
    public LayerMask enemyMask;
    public LayerMask obstacleMask;

    [Header("Spawn Points")]
    public Transform[] points;

    [Header("Timing")]
    public float initialDelay = 0.25f;
    public float spawnDelay = 0.15f;

    [Header("Wave")]
    public int spawnCount = 10;
    public bool loop = false;
    public float loopPause = 3f;

    [Header("Collision Clearance")]
    public float clearRadius = 0.5f;
    public int maxPlacementTries = 8;
    public float jitterRadius = 0.6f;

    [Header("Alive Cap")]
    public int maxAlive = 25;

    [Header("Activation (player proximity)")]
    public float activationRadius = 10f;
    public Transform player;

    int _alive = 0;
    int _spawned = 0;

    void OnValidate()
    {
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
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p)
            {
                player = p.transform;
            }
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        do
        {
            _alive = 0;
            _spawned = 0;

            while (_spawned < spawnCount)
            {
                if (_alive >= maxAlive)
                {
                    yield return null;
                    continue;
                }

                bool didSpawn = TrySpawnOne();

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

            if (loop)
            {
                yield return new WaitForSeconds(loopPause);
            }
        }
        while (loop);
    }

    bool TrySpawnOne()
    {
        if (!enemyPrefab || points == null || points.Length == 0)
        {
            return false;
        }

        Transform spawnPoint;
        if (!PickActiveSpawnPoint(out spawnPoint))
        {
            return false;
        }

        Vector2 finalPos;
        if (!TryFindClearSpot(spawnPoint.position, out finalPos))
        {
            return false;
        }

        var go = Instantiate(enemyPrefab, finalPos, Quaternion.identity, parentForEnemies ? parentForEnemies : null);
        _alive++;

        var ticket = go.AddComponent<EnemySpawnTicket>();
        ticket.owner = this;

        return true;
    }

    bool PickActiveSpawnPoint(out Transform chosen)
    {
        chosen = null;
        if (points == null || points.Length == 0)
        {
            return false;
        }

        if (!player)
        {
            chosen = points[Random.Range(0, points.Length)];
            return chosen != null;
        }

        float radiusSqr = activationRadius * activationRadius;
        int start = Random.Range(0, points.Length);

        for (int offset = 0; offset < points.Length; offset++)
        {
            int index = (start + offset) % points.Length;
            var pt = points[index];
            if (!pt)
            {
                continue;
            }

            Vector2 diff = (Vector2)pt.position - (Vector2)player.position;
            if (diff.sqrMagnitude <= radiusSqr)
            {
                chosen = pt;
                return true;
            }
        }

        return false;
    }

    public void NotifyEnemyGone()
    {
        _alive = Mathf.Max(0, _alive - 1);
    }

    bool TryFindClearSpot(Vector2 center, out Vector2 pos)
    {
        if (IsClear(center))
        {
            pos = center;
            return true;
        }

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

        pos = center;
        return false;
    }

    bool IsClear(Vector2 p)
    {
        if (Physics2D.OverlapCircle(p, clearRadius, enemyMask))
        {
            return false;
        }

        if (obstacleMask.value != 0 && Physics2D.OverlapCircle(p, clearRadius, obstacleMask))
        {
            return false;
        }
        return true;
    }

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

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(t.position, clearRadius);

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(t.position, jitterRadius);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(t.position, activationRadius);
            }
        }
    }
}
