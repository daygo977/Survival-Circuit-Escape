using UnityEngine;
using UnityEngine.SceneManagement;  //loading Main Menu
using UnityEngine.EventSystems;     //selecting Resume Button
using UnityEngine.UI;               //for button
using TMPro;

public class GameStateManager : MonoBehaviour
{

    [Header("Pause UI")]
    [SerializeField] private GameObject pause;      //UI Pause screen
    [SerializeField] private Button resumeButton;   // Resume button
    [SerializeField] private Button pauseMainMenuButton; //Pause Main Menu button
    [SerializeField] private Button pauseQuitButton;     //Pause Quit to desktop button

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOver;    //UI Game Over screen
    [SerializeField] private Button retryButton;     //Retry button
    [SerializeField] private Button gameOverMainMenuButton; //Main menu button from game over screen
    [SerializeField] private Button gameOverQuitButton;     //Pause Quit to desktop button

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Load main menu scene
    [SerializeField] private string gameSceneName = "Game"; //For retry button

    bool paused = false;
    bool gameOvered = false;

    void Awake()
    {
        //Make sure both are hidden at start
        if (pause) pause.SetActive(false);
        if (gameOver) gameOver.SetActive(false);

        //Make sure game is running correctly at start (not frozen)
        Time.timeScale = 1f;
        
        //Hooked up pause buttons and game over buttons to events
        if (resumeButton) resumeButton.onClick.AddListener(OnResumePressed);
        if (pauseMainMenuButton) pauseMainMenuButton.onClick.AddListener(OnMainMenuPressed);
        if (pauseQuitButton) pauseQuitButton.onClick.AddListener(OnQuitPressed);

        if (retryButton) retryButton.onClick.AddListener(OnRetryPressed);
        if (gameOverMainMenuButton) gameOverMainMenuButton.onClick.AddListener(OnMainMenuPressed);
        if (gameOverQuitButton) gameOverQuitButton.onClick.AddListener(OnQuitPressed);
    }

    void Update()
    {
        //Controller implementation later
        //If game over, true, (ESC) leads to main menu
        if (gameOvered)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnMainMenuPressed();
            }
            return;
        }

        //If not, (ESC) toggles pause, or if in pause, resumes game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        paused = true;

        //freeze gameplay
        Time.timeScale = 0f;
        //show pause screen
        pause.SetActive(true);

        //For controller implementation later on
        if (EventSystem.current != null && resumeButton != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }

    private void ResumeGame()
    {
        paused = false;
        //Unfreeze game
        Time.timeScale = 1f;
        //Hide pause screen
        pause.SetActive(false);
    }

    private void OnResumePressed()
    {
        ResumeGame();
    }

    private void OnMainMenuPressed()
    {
        //Make sure time is set to normal
        Time.timeScale = 1f;
        paused = false;
        gameOvered = false;
        //Load main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnQuitPressed()
    {
        //Will exit build
        Application.Quit();
    }

    public void TriggerGameOver()
    {
        //Already game overed
        if (gameOvered) return;

        //Set game overed true, and force paused to false (for ESC key)
        gameOvered = true;
        paused = false;

        //Freeze game
        Time.timeScale = 0f;

        //If Pause UI is true, hide it, and if Game Over UI true, show it
        if (pause) pause.SetActive(false);
        if (gameOver) gameOver.SetActive(true);

        //Focus retry button, for controller implementation later.
        if (retryButton && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }
    }

    public void OnRetryPressed()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(gameSceneName);
    }
}
