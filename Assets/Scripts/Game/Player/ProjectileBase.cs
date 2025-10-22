using UnityEngine;

/// Base Projectile: moves straight, damages on trigger, auto-despawns
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileBase : MonoBehaviour
{
    [Header("Visuals")]
    public bool alignToVelocity = true;     //Rotate to face travel
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
        col.isTrigger = true;           //Triggers for OnTriggerEnter2D
    }

    public virtual void Fire(Vector2 position, Vector2 direct, int dmg, float spd, float lifeTime, LayerMask mask)
    {
        transform.position = position;

        //Aim sprites nose to direction using atan2 (x, y), to radians to degrees (+ offset)
        float angle = Mathf.Atan2(direct.y, direct.x) * Mathf.Rad2Deg + spriteAngleOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        //Set runtime paramters
        damage = dmg;
        speed = spd;
        lifetime = lifeTime;
        hitMask = mask;
        life = lifetime;

        //Move in the normalized direction at speed
        gameObject.SetActive(true);
        rb.velocity = direct.normalized * speed;
    }

    public virtual void Update()
    {
        //Life time coutdown and clean up
        life -= Time.deltaTime;
        if (life <= 0f)
        {
            Despawn();
        }

        //Keep sprite aligned to travel vector direction
        if (alignToVelocity && rb && rb.velocity.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg + spriteAngleOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, ang);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        //Layer check, only damage if other is in hitmask
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        var h = other.GetComponent<EnemyHealth>();
        if (h)
        {
            h.EnemyTakeDamage(damage);

            Despawn();      //remove projectile on hit
        }
    }

    protected void Despawn()
    {
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
