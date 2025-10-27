using UnityEngine;

/// <summary>
/// Component is attached at runtime when each enemy is spawned
/// - When this enemy is disabled or destroyed, tell spawner, one less alive
/// 
/// Needed, since spawner has cap of maxAlive. Without this, spawner won't decrement cap, so
/// it always assumes that cap is hit/full, and no replacements occur. This ticket will report enemy
/// death once.
/// 
/// Process:
/// EnemySpawner.TrySpawnOne() -> Instantiate(enemyPrefab) -> AddComponent<EnemySpawnTicket>()
/// EnemySpawnTicket.owner = that spawner
/// When enemy dies, ReportGone() triggers owner.NotifyEnemyGone()
/// </summary>
public class EnemySpawnTicket : MonoBehaviour
{
    public EnemySpawner owner;      //Which spawner is notified

    bool reported = false;          //Guard so we report once

    /// Internal helper:
    /// - Mark this enemy gone only once
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

    /// Unity calls OnDisable(), when game object or parent disabled.
    void OnDisable()
    {
        if (owner)
        {
            ReportGone();
        }
    }

    /// Unity calls OnDestroy(), when game object or parent is destroyed.
    void OnDestroy()
    {
        if (owner)
        {
            ReportGone();
        }
    }
}
