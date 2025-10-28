using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    //New (10/28/2025)
    [SerializeField] private Button easyButton;

    /// Play button pressed
    public void OnPlayPressed()
    {
        //Hide title, play and quit
        //Show text (Select Difficulty) and game difficulty
        mainGroup.SetActive(false);
        difficultyGroup.SetActive(true);

        //New (10/28/2025)
        // Make sure event system has a live selection so gamepad can navigate
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(easyButton.gameObject);
    }

    /// Quit button pressed
    public void OnQuitPressed()
    {
        Application.Quit();
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
        GlobalDifficulty.current = GlobalDifficulty.Difficulty.Easy;

        GlobalDifficulty.playerStartingHP = 3;
        GlobalDifficulty.playerStartingLives = 3;

        GlobalDifficulty.enemySpeedMul = 0.8f;
        GlobalDifficulty.enemyAttackRange = 0.9f;

        LoadGame();
    }

    /// Normal button pressed
    public void OnNormalPressed()
    {
        //2 lives, regular speed and reach
        GlobalDifficulty.current = GlobalDifficulty.Difficulty.Normal;

        GlobalDifficulty.playerStartingHP = 3;
        GlobalDifficulty.playerStartingLives = 2;

        GlobalDifficulty.enemySpeedMul = 1f;
        GlobalDifficulty.enemyAttackRange = 1f;

        LoadGame();
    }

    /// Hard button pressed
    public void OnHardPressed()
    {
        //2 lives, quicker enemies, more reach
        GlobalDifficulty.current = GlobalDifficulty.Difficulty.Hard;

        GlobalDifficulty.playerStartingHP = 3;
        GlobalDifficulty.playerStartingLives = 2;

        GlobalDifficulty.enemySpeedMul = 1.2f;
        GlobalDifficulty.enemyAttackRange = 1.1f;

        LoadGame();
    }

    /// Nightmare button pressed
    public void OnNightmarePressed()
    {
        //0 lives, fast enemies, longer attack range
        GlobalDifficulty.current = GlobalDifficulty.Difficulty.Nightmare;

        GlobalDifficulty.playerStartingHP = 3;
        GlobalDifficulty.playerStartingLives = 0;

        GlobalDifficulty.enemySpeedMul = 1.5f;
        GlobalDifficulty.enemyAttackRange = 1.2f;

        LoadGame();
    }
}
