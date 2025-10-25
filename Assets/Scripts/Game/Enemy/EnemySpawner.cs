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

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        do
        {
            _alive = 0;
            _spawned = 0;

            while (_spawned < spawnCount)
            {
                if (_alive < maxAlive)
                {
                    SpawnOne(_spawned);
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

    void SpawnOne(int i)
    {
        if (!enemyPrefab || points == null || points.Length == 0)
        {
            return;
        }

        Transform p = points[i % points.Length];

        Vector2 pos;
        if (!TryFindClearSpot(p.position, out pos))
        {
            StartCoroutine(SpawnDelayedRetry(i));
            return;
        }

        var go = Instantiate(enemyPrefab, pos, Quaternion.identity, parentForEnemies ? parentForEnemies : null);
        _alive++;

        var ticket = go.AddComponent<EnemySpawnTicket>();
        ticket.owner = this;
    }

    public void NotifyEnemyGone()
    {
        _alive = Mathf.Max(0, _alive - 1);
    }

    IEnumerator SpawnDelayedRetry(int i)
    {
        yield return null;
        SpawnOne(i);
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

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (points != null)
        {
            foreach (var t in points)
            {
                if (t)
                {
                    Gizmos.DrawSphere(t.position, clearRadius);
                }
            }
        }

        Gizmos.color = Color.magenta;
        if (points != null)
        {
            foreach (var t in points)
            {
                if (t)
                {
                    Gizmos.DrawSphere(t.position, jitterRadius);
                }
            }
        }
    }
}
