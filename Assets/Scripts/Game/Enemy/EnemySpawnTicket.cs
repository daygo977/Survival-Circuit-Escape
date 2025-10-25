using UnityEngine;

public class EnemySpawnTicket : MonoBehaviour
{
    public EnemySpawner owner;

    bool reported = false;

    void ReportGone()
    {
        if (reported)
        {
            return;
        }
        reported = true;

        if (owner != null)
        {
            owner.NotifyEnemyGone();
        }
    }

    void OnDisable()
    {
        if (owner)
        {
            ReportGone();
        }
    }

    void OnDestroy()
    {
        if (owner)
        {
            ReportGone();
        }
    }
}
