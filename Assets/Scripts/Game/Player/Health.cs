using UnityEngine;

public class Health : MonoBehaviour
{
    public int hp = 3;

    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
