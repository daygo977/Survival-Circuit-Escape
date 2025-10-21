using UnityEngine;

public class RocketProjectile : ProjectileBase
{
    public float explosionRadius = 2.5f;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitMask);
        foreach (var c in hits)
        {
            var h = c.GetComponent<Health>();
            if (h)
            {
                h.TakeDamage(damage);
            }
        }
        base.OnTriggerEnter2D(other);
    }
}
