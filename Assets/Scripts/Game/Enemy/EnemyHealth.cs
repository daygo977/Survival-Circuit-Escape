using UnityEngine;


/// Health component for enemies"
/// - subtract HP on damage
/// - destroys game object when HP is 0
/// (Useful for testing weapon stats)
public class EnemyHealth : MonoBehaviour
{
    public int hp = 3;

    [Header("Death behavior")]
    public float destroyDelay = 0.2f;   // Seconds before actual destroy


    public void EnemyTakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            // Scheduled destruction
            Destroy(gameObject, destroyDelay);
        }
    }

}
