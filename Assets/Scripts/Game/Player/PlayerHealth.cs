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

    //New (10/27/2025)
    // Add gizmo hurtbox (circle) to help with player hurtbox size tuning
    [Header("Hurtbox (debug/tuning)")]
    [Tooltip("Radius of the player's hurt circle in world units")]
    public float hurtRadius = 0.5f;
    [Tooltip("Offset from the player's transform.position, in world units. Use to move circle up to match the body")]
    public Vector2 hurtOffset = Vector2.zero;
    [Tooltip("Color to visualize the hurt circle in Scene")]
    public Color hurtColor = new Color(1f, 0f, 0f, 0.25f);

    void Awake()
    {
        // Initialize current state from configured stats
        CurrentHP = maxHp;
        Lives = startingLives;

        //New (10/27/2025)
        //initial HUD state at game start
        HUDManager.SetHealthOnly(Lives, CurrentHP, maxHp);
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

                var rb = GetComponent<Rigidbody2D>();
                if (rb)
                {
                    rb.velocity = Vector2.zero;
                }
                
                //New (10/28/2025)
                //When player out of lives and HP, after disables trigger game over screen
                var gameOverUI = FindObjectOfType<GameStateManager>();
                if (gameOverUI != null) gameOverUI.TriggerGameOver();

            }
        }
        else
        {
            // Got damaged, but still have HP
            StartInvul();
        }

        //New (10/27/2025)
        //push latest lives/HP, even if player can't move and weapon systems are disabled
        HUDManager.SetHealthOnly(Lives, CurrentHP, maxHp);
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

    //New (10/27/2025)
    //Draw hurt circle in Scene when player is selected
    private void OnDrawGizmos()
    {
        //Compute world=space center of the hurt circle
        //player position + offset in inspector
        Vector3 center = transform.position + (Vector3)hurtOffset;

        //Outline
        Gizmos.color = hurtColor.a > 0f ? new Color(hurtColor.r, hurtColor.g, hurtColor.b, 1f) : Color.red;
        Gizmos.DrawWireSphere(center, hurtRadius);
    }

    /// New (10/28/2025)
    /// On start apply difficulty stats
    void Start()
    {
        //Get stats from button pressed (difficulty buttons)
        startingLives = GlobalDifficulty.playerStartingLives;
        maxHp = GlobalDifficulty.playerStartingHP;

        Lives = startingLives;
        CurrentHP = maxHp;

        HUDManager.SetHealthOnly(Lives, CurrentHP, maxHp);

    }
}
