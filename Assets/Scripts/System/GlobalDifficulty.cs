using UnityEngine;

/// <summary>
/// Static holder for the game runs chosen difficulty (tuning)
/// The Game scene will read these values on Awake/Start and apply them
/// (player starting lives, enemy speed multipliers, etc.)
/// </summary>
public class GlobalDifficulty
{
    //How many lives player start with
    //Nightmare mode, player will not have any additional lives
    public static int startingLives = 2;

    //Enemy chase speed multiplier
    //Normal = 1f, Slow < 1f, Hard > 1f
    public static float enemySpeedMul = 1f;

    //Enemy attack range multipler.
    //Will be the same except in nightmare, its increased
    public static float enemyAttackRange = 1f;
    
    //Hardcore: true for nightmare
    public static bool hardcore = false;
}
