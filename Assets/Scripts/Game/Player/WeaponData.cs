using UnityEngine;

public enum WeaponKind { Hitscan, Projectile, Melee }

[CreateAssetMenu(menuName = "SCE/Weapon Data", fileName = "NewWeapon")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Weapon";

    [Header("Kind")]
    public WeaponKind kind = WeaponKind.Hitscan;

    [Header("Firing")]
    [Tooltip("Bulleys per second or swings per second")]
    public float fireRate = 8f;
    [Tooltip("Damage per ray/pellet/projectile/swing")]
    public int damage = 1;

    [Header("Spread & Pellets (Hitscan or Projectile shotguns)")]
    [Tooltip("Number of rays/pellets per shot (shotgun) and 1 for rifles/SMG")]
    public int pellets = 1;
    [Tooltip("Cone angle in degrees applied per pellet/ray")]
    public float spreadDegrees = 0f;

    [Header("Ranges")]
    [Tooltip("Max range for hitscan rays and melee reach")]
    public float range = 12f;

    [Header("Masks")]
    public LayerMask hitMask;       //Layers that can be damaged
    public LayerMask blockMask;     //Layers that stop rays (used for walls)

    [Header("Projectile (RPG, grenades, etc.)")]
    public ProjectileBase projectilePrefab;
    public float projectileSpeed = 18f;
    public float projectileLifeTime = 3f;

    [Header("Melee")]
    [Tooltip("Degrees of the swing arc (centered on aim)")]
    public float meleeArcDegrees = 100f;
    [Tooltip("How far from the player the swing reaches")]
    public float meleeRange = 1.6f;

    //New(10/29/2025)
    //Audio Fields
    [Header("Audio - Fire/Swing")]
    [Tooltip("Sound to play when weapon fires (hitscan) or swings. For AR and SMG can use same Audio")]
    public AudioClip fireSFX;
    [Range(0f, 1f)]
    public float fireVolume = 0.4f;

    [Tooltip("Reference fire rate for pitch math. fire rate vs this gives pitch. Example: 10 means 10 shots/sec sounds pitchBase.")]
    public float pitchRefFireRate = 10f;

    [Tooltip("Base pitch (1 = normal). We auto-raise/lower around this based on fire rate")]
    public float pitchBase = 1f;

    [Tooltip("Max +/- pitch range allowed by scaling. Example: 0.4 means base-0.4 and base+0.4")]
    public float pitchClamp = 0.3f;

    [Header("Audio - Projectile (RPG)")]
    [Tooltip("Play once when a projectile is spawned/launched")]
    public AudioClip launchSFX;
    [Range(0f, 1f)]
    public float launchVolume = 0.4f;

    [Tooltip("Played when projectile impact/explosion")]
    public AudioClip explosionAudio;
    [Range(0f, 1f)]
    public float explosionVolume = 0.4f;
}
