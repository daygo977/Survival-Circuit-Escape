using UnityEngine;


public class EnemyHealth : MonoBehaviour
{
    public int hp = 3;

    public void EnemyTakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            gameObject.SetActive(false);
        }
    }

}
