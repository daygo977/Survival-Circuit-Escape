using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Base Projectile: moves straight, damages on trigger, auto-despawns
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileBase : MonoBehaviour
{
    [Header("Visuals")]
    public bool alignToVelocity = true;
    [Tooltip("Degrees to add so the sprite's nose points along travel")]
    public float spriteAngleOffset = 0f;

    public int damage = 1;
    public float speed = 10f;
    public float lifetime = 2f;
    public LayerMask hitMask;

    Rigidbody2D rb;
    float life;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public virtual void Fire(Vector2 position, Vector2 direct, int dmg, float spd, float lifeTime, LayerMask mask)
    {
        transform.position = position;

        float angle = Mathf.Atan2(direct.y, direct.x) * Mathf.Rad2Deg + spriteAngleOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        damage = dmg;
        speed = spd;
        lifetime = lifeTime;
        hitMask = mask;
        life = lifetime;
        gameObject.SetActive(true);
        rb.velocity = direct.normalized * speed;
    }

    public virtual void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f)
        {
            Despawn();
        }

        if (alignToVelocity && rb && rb.velocity.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg + spriteAngleOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, ang);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        var h = other.GetComponent<Health>();
        if (h)
        {
            h.TakeDamage(damage);

            Despawn();
        }
    }

    protected void Despawn()
    {
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
