using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 3;
    public int startingLives = 2;

    [Header("Damage/I-frames")]
    public float invulTime = 0.75f;

    public int CurrentHP { get; private set; }
    public int Lives { get; private set; }

    bool invul;
    float invulTimer;

    void Awake()
    {
        CurrentHP = maxHp;
        Lives = startingLives;
    }

    public void PlayerTakeDamage(int dmg)
    {
        if (invul || dmg <= 0)
        {
            return;
        }

        CurrentHP = Mathf.Max(0, CurrentHP - dmg);

        if (CurrentHP == 0)
        {
            if (Lives > 0)
            {
                Lives--;
                CurrentHP = maxHp;
                StartInvul();
            }
            else
            {
                var move = GetComponent<PlayerMovement>();
                if (move != null)
                {
                    move.enabled = false;
                }

                var gun = GetComponent<WeaponSystem>();
                if (gun != null)
                {
                    gun.enabled = false;
                }

            }
        }
        else
        {
            StartInvul();
        }
    }

    void StartInvul()
    {
        invul = true;
        invulTimer = invulTime;
    }

    void Update()
    {
        if (!invul)
        {
            return;
        }
        invulTimer -= Time.deltaTime;
        if (invulTimer <= 0f)
        {
            invul = false;
        }
    }
}
