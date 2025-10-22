using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaser : MonoBehaviour
{
    [Header("Chase")]
    public float moveSpeed = 5f;
    public float slowRadius = 1.5f;
    public float stopDistance = 0.35f;

    [Header("Avoidance (for walls)")]
    public LayerMask obstacleMask;
    public float feelerLength = 1.2f;
    public float sideAngle = 30f;
    public float avoidWeight = 2f;

    [Header("Attack")]
    public float attackRange = 0.6f;
    public int contactDamage = 1;
    public float attackCooldowm = 0.7f;

    Transform target;
    Rigidbody2D rb;
    PlayerHealth playerHealth;
    float cd;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO)
        {
            target = playerGO.transform;
            playerHealth = playerGO.GetComponent<PlayerHealth>();
        }
    }

    void FixedUpdate()
    {
        if (!target)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = (target.position - transform.position);
        float dist = toPlayer.magnitude;
        Vector2 direct;
        if (dist > 0.001f)
        {
            direct = toPlayer / dist;
        }
        else
        {
            direct = Vector2.zero;
        }

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

        Vector2 avoid = Vector2.zero;
        avoid += AvoidInDirection(direct);
        avoid += AvoidInDirection(Rotate(direct, +sideAngle));
        avoid += AvoidInDirection(Rotate(direct, -sideAngle));
        avoid *= avoidWeight;

        Vector2 finalDirect = (direct + avoid).normalized;
        rb.velocity = finalDirect * desiredSpeed;

        cd -= Time.fixedDeltaTime;
        if (dist <= attackRange && cd <= 0f && playerHealth != null)
        {
            playerHealth.PlayerTakeDamage(contactDamage);
            cd = attackCooldowm;
        }
    }

    Vector2 AvoidInDirection(Vector2 d)
    {
        if (feelerLength <= 0f || obstacleMask.value == 0)
        {
            return Vector2.zero;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, d, feelerLength, obstacleMask);

        if (!hit)
        {
            return Vector2.zero;
        }

        float strength = 1f - (hit.distance / feelerLength);
        return hit.normal * Mathf.Clamp01(strength);
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cn = Mathf.Cos(rad), sn = Mathf.Sin(rad);
        return new Vector2(v.x * cn - v.y * sn, v.x * sn + v.y * cn);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackRange > 0f)
        {
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
