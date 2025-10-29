using UnityEngine;

public class RocketProjectile : ProjectileBase
{
    public float explosionRadius = 2.5f;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //Ignore if other layer not in hitmask
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        //AoE: collect all targets in circle radius (explosion) and apply damage to them
        var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitMask);
        foreach (var c in hits)
        {
            var h = c.GetComponent<EnemyHealth>();
            if (h)
            {
                h.EnemyTakeDamage(damage);
            }
        }

        //New (10/29/2025)
        //Play explosion SFX at impact point
        PlayOneShotAtPos(explosionClip, explosionVol, transform.position);

        base.OnTriggerEnter2D(other);   //despawns itself via ProjectileBase.cs
    }
}
