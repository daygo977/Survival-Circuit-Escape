using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaser : MonoBehaviour
{
    // Enemy movement/chase tuning
    [Header("Chase")]
    public float moveSpeed = 5f;        // Max chase speed (needs to match player speed)
    public float slowRadius = 1.5f;     // Slowdown distance when close to player
    public float stopDistance = 0.35f;  // Stop when distance to player has been met (prevents jitter)

    // Obstacle avoidance (using rays "feeler")
    [Header("Avoidance (for walls)")]
    public LayerMask obstacleMask;      // Obstacle to treat as wall
    public float feelerLength = 1.2f;   // Raycast length in front/side directions
    public float sideAngle = 30f;       // Side feeler angles
    public float avoidWeight = 2f;      // Determines how strong avoidance influences steering

    // Attack tuning
    [Header("Attack")]
    public float attackRange = 0.6f;    // Can hit player within radius
    public int contactDamage = 1;       // Damage per hit
    public float attackCooldowm = 0.7f; // Time between hits (prevent same enemy from attacking too quickly)

    // Crowd Seperation (prevents stacking)
    [Header("Crowd Seperation")]
    public LayerMask enemyMask;             // Layer for other enemies
    public float seperationRadius = 0.8f;   // Radius to find neighbors for seperation
    public float seperationWeight = 1.5f;   // Strength for seperation steering
    static readonly Collider2D[] _neighbors = new Collider2D[16];   // Small buffer

    // Runtime references/states
    Transform target;           // Player transform (chase target)
    Rigidbody2D rb;             // Player body movement
    PlayerHealth playerHealth;
    float cd;                   // Attack cooldown timer

    void Awake()
    {
        // Cache body and lock physics that would cause collisions with steering
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        // Find player at the start (expects player object to have tag "Player")
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO)
        {
            target = playerGO.transform;
            playerHealth = playerGO.GetComponent<PlayerHealth>();
        }
    }

    void FixedUpdate()
    {
        // If no target, then stop
        if (!target)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Direction to player/distance
        Vector2 toPlayer = target.position - transform.position;
        float dist = toPlayer.magnitude;

        // Normalized forward direction
        Vector2 direct;
        if (dist > 0.001f)
        {
            direct = toPlayer / dist;
        }
        else
        {
            direct = Vector2.zero;
        }

        // Desired speed (slow down when close, stop when within stop distance)
        float desiredSpeed;
        if (dist < slowRadius)
        {
            desiredSpeed = Mathf.Lerp(0f, moveSpeed, dist / slowRadius);
        }
        else
        {
            desiredSpeed = moveSpeed;
        }

        if (dist <= stopDistance)
        {
            desiredSpeed = 0f;
        }

        // Obstacle avoidance, cast rays forward/sides, add push away from walls
        Vector2 avoid = Vector2.zero;
        avoid += AvoidInDirection(direct);  // forward feeler
        avoid += AvoidInDirection(Rotate(direct, +sideAngle));  // right feeler
        avoid += AvoidInDirection(Rotate(direct, -sideAngle));  // left feeler
        avoid *= avoidWeight;

        // Seperation, push away from nearby enemies (in enemyMask)
        Vector2 sep = Vector2.zero;
        int n = Physics2D.OverlapCircleNonAlloc(transform.position, seperationRadius, _neighbors, enemyMask);
        for (int i = 0; i < n; i++)
        {
            var c = _neighbors[i];
            if (!c || c.attachedRigidbody == rb)
            {
                continue;   // skip self or invalid
            }

            Vector2 away = (Vector2)(transform.position - c.transform.position);
            float d2 = away.sqrMagnitude;   // inverse-square weighting (stronger when really close)
            if (d2 > 0.0001f)
            {
                sep += away.normalized / Mathf.Max(d2, 0.1f);
            }
        }
        if (sep != Vector2.zero)
        {
            sep = sep.normalized * seperationWeight;
        }

        // Combine steering and apply velocity
        Vector2 finalDirect = (direct + avoid + sep).normalized;
        rb.velocity = finalDirect * desiredSpeed;

        // Attack cooldown
        cd -= Time.fixedDeltaTime;
        if (dist <= attackRange && cd <= 0f && playerHealth != null)
        {
            // Apply damage to the player and reset the cool down
            playerHealth.PlayerTakeDamage(contactDamage);
            cd = attackCooldowm;
        }
    }

    /// Casts ray in direction 'd' and return push vector away from the hit surface.
    /// Strength will increase when closer to hit obstacle
    Vector2 AvoidInDirection(Vector2 d)
    {
        // Skip if no feeler length or no obstacle layer is set
        if (feelerLength <= 0f || obstacleMask.value == 0)
        {
            return Vector2.zero;
        }
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, d, feelerLength, obstacleMask);
        if (!hit)
        {
            return Vector2.zero;
        }

        // 1 at contact, 0 at the end of the feeler
        float strength = 1f - (hit.distance / feelerLength);

        // Push along hit normal (away from surface)
        return hit.normal * Mathf.Clamp01(strength);
    }

    /// Rotates 2D vector by degrees around z
    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cn = Mathf.Cos(rad), sn = Mathf.Sin(rad);
        return new Vector2(v.x * cn - v.y * sn, v.x * sn + v.y * cn);
    }

    void OnDrawGizmosSelected()
    {
        // Attack range (red) and seperation radius (yellow) for tunning
        Gizmos.color = Color.red;
        if (attackRange > 0f)
        {
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, seperationRadius);
    }
}
