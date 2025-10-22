using UnityEngine;

/// Tracks player's HP and lives, with each time the player is hit, the player gains I-frames
/// (short window of invulnerability). When out of lives, the players weapons and movement are disabled.
public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 3;           //HP per life
    public int startingLives = 2;   //How many lifes the player starts with

    [Header("Damage/I-frames")]
    public float invulTime = 0.75f; //seconds of I-frames hit/respawn

    // Read-only state other systems can utilize
    public int CurrentHP { get; private set; }
    public int Lives { get; private set; }

    // I-frame state
    bool invul;         // True while invulnerable
    float invulTimer;   // Remaining I-frame time

    void Awake()
    {
        // Initialize current state from configured stats
        CurrentHP = maxHp;
        Lives = startingLives;
    }

    /// Apply damage to player (ignored if invulnerable or dmg <= 0)
    /// Handles life consumption and game over disables (movement and weapon use are disabled)
    public void PlayerTakeDamage(int dmg)
    {
        // If invulnerable or invalid dmg, do nothing
        if (invul || dmg <= 0)
        {
            return;
        }

        // Stop at 0, can't go below it
        CurrentHP = Mathf.Max(0, CurrentHP - dmg);

        if (CurrentHP == 0)
        {
            // Lost all HP for this life
            if (Lives > 0)
            {
                Lives--;
                CurrentHP = maxHp;
                StartInvul();
            }
            else
            {
                // No lives left, game over, disable player control
                var move = GetComponent<PlayerMovement>();
                if (move != null)
                {
                    move.enabled = false;
                }

                var weapon = GetComponent<WeaponSystem>();
                if (weapon != null)
                {
                    weapon.enabled = false;
                }

            }
        }
        else
        {
            // Got damaged, but still have HP
            StartInvul();
        }
    }

    /// Begin I-frames (prevents getting hit multiple times in a small window)
    void StartInvul()
    {
        invul = true;
        invulTimer = invulTime;
    }

    void Update()
    {
        // Countdown for active I-frames
        if (!invul)
        {
            return;
        }
        invulTimer -= Time.deltaTime;
        if (invulTimer <= 0f)
        {
            // I-frames end, damage can be applied again
            invul = false;
        }
    }
}
