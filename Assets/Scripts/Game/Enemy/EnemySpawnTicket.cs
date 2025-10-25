using UnityEngine;

public class EnemySpawnTicket : MonoBehaviour
{
    public EnemySpawner owner;

    void OnDisable()
    {
        if (owner)
        {
            owner.NotifyEnemyGone();
        }
    }

    void OnDestroy()
    {
        if (owner)
        {
            owner.NotifyEnemyGone();
        }
    }
}
