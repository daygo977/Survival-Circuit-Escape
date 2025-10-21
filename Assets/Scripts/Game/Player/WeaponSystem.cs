using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles weapon firing using WeaponData. Reads aim from PlayerAim.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;           //Player input
    [SerializeField] private string fireActionName = "Fire";       //Action name in action map

    [Header("References")]
    public PlayerAim aim;       //Helps give normalized aim direction
    public Transform muzzle;    //Where the shots will come from (child of player)

    [Header("Loadout")]
    public List<WeaponData> loadout = new();        //Order equals slot order for switching
    public int currentIndex = 0;                    //Currently equipped slot

    [Header("VFX (optional)")]
    public LineRenderer tracerPrefab;       //short-lived line for hitscan visuals
    public float tracerDuration = 0.025f;      //Tracer duration

    [Header("VFX - Melee")]
    public MeleeArcVFX meleeArcPrefab;      //short-lived arc visual (wedges) for melee visuals

    float cooldown;     //time until next shot is allowed to fire
    bool isFiring;      //current fire button state (pressed, held, no pressed/held)

    void Awake()
    {
        //Auto-bind components when nothing is assigned to them
        if (!playerInput)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (!aim)
        {
            aim = GetComponent<PlayerAim>();
        }

        //Auto-create muzzle if not assigned yet
        if (!muzzle)
        {
            muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(transform);
            muzzle.localPosition = new Vector3(0f, 0.5f, 0f);
        }
    }

    /// Toggle small muzzle flash vfx (temporarly a orange circle) if present
    void PlayMuzzleFlash()
    {
        if (!muzzle)
        {
            return;
        }

        //Looks under muzzle for a disabled MuzzleFlash object (child) and enables it briefly
        var flash = muzzle.GetComponentInChildren<MuzzleFlash>(true);

        if (flash)
        {
            flash.gameObject.SetActive(true);
        }
    }

    /// Change equipped weapon slot safely and reset shot cooldown (fix later on, rpg can be spammed)
    void SetIndex(int i)
    {
        currentIndex = Mathf.Clamp(i, 0, Mathf.Max(0, loadout.Count - 1));
        cooldown = 0f;  //
    }

    /// Input System (send messages): next/prev weapon bindings
    public void OnNextWeapon(InputValue _)
    {
        SetIndex((currentIndex + 1) % loadout.Count);
    }

    public void OnPrevWeapon(InputValue _)
    {
        SetIndex((currentIndex - 1 + loadout.Count) % loadout.Count);
    }

    /// Starting point, helps route to correct firing mode for the active weapon
    void Fire(WeaponData w)
    {
        // Reads direction from PlayerAim 
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

        // Checks the weapon kind
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

    /// Hitscan, cast rays instantly, has the option for rays to be blocked by walls (blockmask), one tracer per bullet
    void FireHitscan(WeaponData w, Vector2 origin, Vector2 direct)
    {
        PlayMuzzleFlash();      //Visual line at the muzzle

        int pellets = Mathf.Max(1, w.pellets);
        for (int i = 0; i < pellets; i++)
        {
            //Spread, rotate direction by a small random angle within +- spread/2
            //angle in radians to (cos, sin) build a 2D rotation
            Vector2 d = ApplySpread(direct, w.spreadDegrees);

            //If blockmask is on, combine hitmask so walls stop rays
            RaycastHit2D hit;
            if (w.blockMask.value != 0)
            {
                hit = Physics2D.Raycast(origin, d, w.range, w.hitMask | w.blockMask);
            }
            else
            {
                hit = Physics2D.Raycast(origin, d, w.range, w.hitMask);
            }

            // Default end equals to max range along d (shows tracer range if nothing is hit)
            Vector2 end = origin + d * w.range;

            if (hit.collider != null)
            {
                //If the first thing we hit is damageable, apply the correct amount of damage
                var h = hit.collider.GetComponent<Health>();
                if (h)
                {
                    h.TakeDamage(w.damage);
                }

                end = hit.point;    //Cut tracer to impact
            }

            if (tracerPrefab)
            {
                StartCoroutine(SpawnTracer(origin, end));
            }
        }
    }

    /// Projectile, instantiate a projectile prefab and lit it steer itself (go straight in the direction of initial fire)
    void FireProjectile(WeaponData w, Vector2 origin, Vector2 direct)
    {
        int pellets = Mathf.Max(1, w.pellets);
        for (int i = 0; i < pellets; i++)
        {
            Vector2 d = ApplySpread(direct, w.spreadDegrees);
            var p = Instantiate(w.projectilePrefab);

            // Hand off movement, rotation, lifetime, masks to projectile
            p.Fire(origin, d, w.damage, w.projectileSpeed, w.projectileLifeTime, w.hitMask);
        }
    }

    /// Melee, overlap a circle, them keep only targets inside the swing arc (wedges)
    void DoMelee(WeaponData w, Vector2 origin, Vector2 direct)
    {
        //Get targets in a circle centered slightly along the aim direction
        var hits = Physics2D.OverlapCircleAll(origin + direct * (w.meleeRange * 0.6f), w.meleeRange, w.hitMask);    //degrees, symmetric wedges along direction
        float halfArc = w.meleeArcDegrees * 0.5f;

        foreach (var c in hits)
        {
            // Vector from player to target (normalized for angle math)
            Vector2 to = ((Vector2)c.transform.position - origin).normalized;

            // Angle (direction, to) is less than or equal to halfarc, then target is inside the swing wedge
            if (Vector2.Angle(direct, to) <= halfArc)
            {
                var h = c.GetComponent<Health>();
                if (h)
                {
                    h.TakeDamage(w.damage);
                }
            }
        }

        // Wedge VFX to show the swing arc
        if (meleeArcPrefab)
        {
            var fx = Instantiate(meleeArcPrefab);
            fx.Show(origin, direct, w.meleeRange, w.meleeArcDegrees);
        }
    }

    /// Rotate direction by a random angle (jitter) within +- (spreadDegrees/2)
    /// Returns a unit vector
    static Vector2 ApplySpread(Vector2 direct, float spreadDegrees)
    {
        if (spreadDegrees <= 0f)
        {
            return direct;
        }

        float ang = Random.Range(-spreadDegrees * 0.5f, spreadDegrees * 0.5f) * Mathf.Deg2Rad; //degrees to radians
        float cs = Mathf.Cos(ang), sn = Mathf.Sin(ang);
        
        //2D rotation:
        //      [x'] = [ cos   -sin][x]
        //      [y'] = [ sin    cos][y]
        return new Vector2(direct.x * cs - direct.y * sn, direct.x * sn + direct.y * cs).normalized;
    }

    void Update()
    {
        //No weapons then do nothing
        if (loadout == null || loadout.Count == 0)
        {
            return;
        }

        // Check the input action every frame so firing is always right
        if (playerInput)
        {
            isFiring = playerInput.actions[fireActionName].IsPressed();
        }

        // Rate-gate, reduce cooldown, and only fire when gate is open
        var w = loadout[currentIndex];
        cooldown -= Time.deltaTime;

        if (isFiring && cooldown <= 0f)
        {
            Fire(w);
            // Convert fireRate (shots/sec) to (sec/shots)
            cooldown = 1f / Mathf.Max(0.01f, w.fireRate);
        }
    }

    /// Safely deafault so firing never sticks/locks up when component toggles
    void OnEnable()
    {
        isFiring = false;
    }

    void OnDisable()
    {
        isFiring = false;
    }

    /// These will get called by Send Messages
    public void OnFireStarted()
    {
        isFiring = true;    //press down
    }

    public void OnFireCanceled()
    {
        isFiring = false;   //release
    }
}
