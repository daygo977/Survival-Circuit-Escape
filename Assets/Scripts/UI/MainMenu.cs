using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles main menu:
/// 1. First screen: Play and Quit
/// 2. After Play: hide Play and Quit, and show difficulty buttons
/// 3. When selecting difficulty: write values into GlobalDifficulty, then load Game scene
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Groups")]
    [SerializeField] private GameObject mainGroup;  //Contains main button group (Title, Play and Quit)
    [SerializeField] private GameObject difficultyGroup;    //Contains difficulty buttons (Text, Easy, Normal, Hard, Nightmare)

    [Header("Scene to Load")]
    [SerializeField] private string gameSceneName = "Game";

    /// Play button pressed
    public void OnPlayPressed()
    {
        //Hide title, play and quit
        //Show text (Select Difficulty) and game difficulty
        mainGroup.SetActive(false);
        difficultyGroup.SetActive(true);
    }

    /// Quit button pressed
    public void OnQuitPressed()
    {
        Application.Quit();
    }

    /// Helper function
    private void SetDifficulty(int lives, float speedMul, float attackRangeMul, bool hardcore)
    {
        GlobalDifficulty.startingLives = lives;
        GlobalDifficulty.enemySpeedMul = speedMul;
        GlobalDifficulty.enemyAttackRange = attackRangeMul;
        GlobalDifficulty.hardcore = hardcore;
    }

    /// Helper function, loads game scene
    private void LoadGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    /// Easy button pressed
    public void OnEasyPressed()
    {
        //3 lives, slow enemies and shorter range
        int lives = 3;
        float speedMul = 0.75f;
        float attackRangeMul = 0.85f;
        bool hardcore = false;

        SetDifficulty(lives, speedMul, attackRangeMul, hardcore);
        LoadGame();
    }

    /// Normal button pressed
    public void OnNormalPressed()
    {
        //2 lives, regular speed and reach
        int lives = 2;
        float speedMul = 1f;
        float attackRangeMul = 1f;
        bool hardcore = false;

        SetDifficulty(lives, speedMul, attackRangeMul, hardcore);
        LoadGame();
    }

    /// Hard button pressed
    public void OnHardPressed()
    {
        //2 lives, quicker enemies, more reach
        int lives = 2;
        float speedMul = 1.15f;
        float attackRangeMul = 1.1f;
        bool hardcore = false;

        SetDifficulty(lives, speedMul, attackRangeMul, hardcore);
        LoadGame();
    }

    /// Nightmare button pressed
    public void OnNightmarePressed()
    {
        //0 lives, fast enemies, longer attack range
        int lives = 0;
        float speedMul = 1.4f;
        float attackRangeMul = 1.25f;
        bool hardcore = true;

        SetDifficulty(lives, speedMul, attackRangeMul, hardcore);
        LoadGame();
    }
}
