using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles weapon firing using WeaponData. Reads aim from PlayerAim.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [Header("References")]
    public PlayerAim aim;
    public Transform muzzle;

    [Header("Loadout")]
    public List<WeaponData> loadout = new();
    public int currentIndex = 0;

    [Header("VFX (optional)")]
    public LineRenderer tracerPrefab;
    public float tracerDuration = 0.025f;

    float cooldown;
    bool isFiring;

    void Awake()
    {
        if (!aim)
        {
            aim = GetComponent<PlayerAim>();
        }

        if (!muzzle)
        {
            muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(transform);
            muzzle.localPosition = new Vector3(0f, 0.5f, 0f);
        }
    }

    public void OnFire(InputValue v)
    {
        isFiring = v.isPressed;
    }

    void SetIndex(int i)
    {
        currentIndex = Mathf.Clamp(i, 0, Mathf.Max(0, loadout.Count - 1));
        cooldown = 0f;
    }

    public void OnNextWeapon(InputValue _)
    {
        SetIndex((currentIndex + 1) % loadout.Count);
    }

    public void OnPrevWeapon(InputValue _)
    {
        SetIndex((currentIndex - 1 + loadout.Count) % loadout.Count);
    }

    void Fire(WeaponData w)
    {
        Vector2 direct;
        if (aim != null)
        {
            direct = aim.AimDirection;
        }
        else
        {
            direct = Vector2.right;
        }

        Vector2 origin = muzzle.position;

        switch (w.kind)
        {
            case WeaponKind.Hitscan:
                FireHitscan(w, origin, direct);
                break;

            case WeaponKind.Projectile:
                FireProjectile(w, origin, direct);
                break;

            case WeaponKind.Melee:
                DoMelee(w, origin, direct);
                break;
        }
    }

    System.Collections.IEnumerator SpawnTracer(Vector2 a, Vector2 b)
    {
        var lr = Instantiate(tracerPrefab);
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        yield return new WaitForSeconds(tracerDuration);
        Destroy(lr.gameObject);
    }

    void FireHitscan(WeaponData w, Vector2 origin, Vector2 direct)
    {
        int pellets = Mathf.Max(1, w.pellets);
        for (int i = 0; i < pellets; i++)
        {
            Vector2 d = ApplySpread(direct, w.spreadDegrees);
            RaycastHit2D hit;

            if (w.blockMask.value != 0)
            {
                hit = Physics2D.Raycast(origin, d, w.range, w.hitMask | w.blockMask);
            }
            else
            {
                hit = Physics2D.Raycast(origin, d, w.range, w.hitMask);
            }

            Vector2 end = origin + d * w.range;

            if (hit.collider != null)
            {
                var h = hit.collider.GetComponent<Health>();
                if (h)
                {
                    h.TakeDamage(w.damage);
                }

                end = hit.point;
            }

            if (tracerPrefab)
            {
                StartCoroutine(SpawnTracer(origin, end));
            }
        }
    }

    void FireProjectile(WeaponData w, Vector2 origin, Vector2 direct)
    {
        int pellets = Mathf.Max(1, w.pellets);
        for (int i = 0; i < pellets; i++)
        {
            Vector2 d = ApplySpread(direct, w.spreadDegrees);
            var p = Instantiate(w.projectilePrefab);
            p.Fire(origin, d, w.damage, w.projectileSpeed, w.projectileLifeTime, w.hitMask);
        }
    }

    void DoMelee(WeaponData w, Vector2 origin, Vector2 direct)
    {
        var hits = Physics2D.OverlapCircleAll(origin + direct * (w.meleeRange * 0.6f), w.meleeRange, w.hitMask);
        float halfArc = w.meleeArcDegrees * 0.5f;

        foreach (var c in hits)
        {
            Vector2 to = ((Vector2)c.transform.position - origin).normalized;
            if (Vector2.Angle(direct, to) <= halfArc)
            {
                var h = c.GetComponent<Health>();
                if (h)
                {
                    h.TakeDamage(w.damage);
                }
            }
        }
    }

    static Vector2 ApplySpread(Vector2 direct, float spreadDegrees)
    {
        if (spreadDegrees <= 0f)
        {
            return direct;
        }

        float ang = Random.Range(-spreadDegrees * 0.5f, spreadDegrees * 0.5f) * Mathf.Deg2Rad;
        float cs = Mathf.Cos(ang), sn = Mathf.Sin(ang);
        return new Vector2(direct.x * cs - direct.y * sn, direct.x * sn + direct.y * cs).normalized;
    }

    void Update()
    {
        if (loadout == null || loadout.Count == 0)
        {
            return;
        }

        var w = loadout[currentIndex];
        cooldown -= Time.deltaTime;

        if (isFiring && cooldown <= 0f)
        {
            Fire(w);
            cooldown = 1f / Mathf.Max(0.01f, w.fireRate);
        }
    }
}
