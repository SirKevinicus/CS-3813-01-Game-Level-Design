using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/**
 * Author: Kevin Gerstner
 * Date Created: 9/12/2020
 * Date Modified: 9/12/2020
 * Description: The Game Manager handles basic game resources like score, health, and UI
 */

public class GameManager : MonoBehaviour
{

 /* +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
  * PUBLIC VARIABLES
  * +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+= */

    //------ GAME INFO ------
    #region Singleton
    private static GameManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Game Manager found.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    [Header("GAME INFORMATION")]
    public string gameTitle; // name of the game
    public string gameCopyright = "Copyright" + year; // date created
    public string gameCredits = "Made by me"; // the text that will appear in the credits

    public string endMessageDisplayWin; // message that will display if you win
    public string endMessageDisplayLose; // message that will display if you lose

    //-------GAME SETTINGS-----------
    [Header("GAME SETTINGS")]

    [Tooltip("Can the level be beat by a certain score?")]
    public bool canBeatLevel;
    public bool timedLevel; // does the level have a timer

    public int beatLevelScore; // score required to beat the level
    public int defaultScore; // default score
    [Range(1, 10)] public int defaultLives; // default lives

    //-------UI-------------
    [Header("COMPONENTS")]

    [Tooltip("Select the GameObject that represents the player.")]
    public GameObject player; // player object

    [Space(10)]
    public Canvas menuCanvas;
    public Canvas hudCanvas;
    public Canvas endScreenCanvas;
    public Canvas footerCanvas;

    [Space(10)]
    public TMP_Text gameTitleDisplay;
    public TMP_Text endGameMessageDisplay;

    [Space(10)]
    public TMP_Text scoreValueDisplay;
    public TMP_Text scoreTitleDisplay;

    [Space(10)]
    public TMP_Text livesValueDisplay;
    public TMP_Text livesTitleDisplay;

    [Space(10)]
    public TMP_Text timerValueDisplay;
    public TMP_Text timerTitleDisplay;

    //-------AUDIO----------
    [Header("AUDIO")]
    public AudioSource backgroundMusic;
    public AudioClip gameOverSFX;
    public AudioClip beatLevelSFX;

 /* +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
  * PRIVATE VARIABLES
  * +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+= */

    //-------STATE----------
    private bool backgroundMusicIsOver;
    private bool gameStarted;
    private bool replay;
    public enum GameStates { Playing, Death, GameOver, BeatLevel }
    [HideInInspector]
    public GameStates gameState;
    private bool isGameOver;
    private bool playerIsDead;

    //-------VARIABLES------
    private static int score;
    private static int lives;

    //-------LEVEL----------
    [Header("LEVELS")]
    public string menuLevel;
    public string gameLevel;
    private string levelToLoad;
    private string currentLevel;

    //-------TIME--------
    private static string year = System.DateTime.Now.ToString("yyyy"); // currrent year
    private float startTime;
    private float currentTime;

    //***********************************************
    // INITIALIZE
    //***********************************************

    private void Start()
    {
        Scene currentLevel = SceneManager.GetActiveScene();
        menuLevel = currentLevel.name;
        HideMenus();
        Reset();
        levelToLoad = menuLevel;
        menuCanvas.gameObject.SetActive(true);
    }

    private void Reset()
    {
        currentTime = 0;
        gameCopyright = "Copyright " + year;
        gameTitleDisplay.text = gameTitle;

        score = defaultScore;
        lives = defaultLives;

        canBeatLevel = false;
        beatLevelScore = 20;
        backgroundMusicIsOver = false;
        gameStarted = false;
        replay = false;
        isGameOver = false;
        playerIsDead = false;

        startTime = 0;
    }

    //***********************************************
    // GAME STATE
    //***********************************************

    public void Quit()
    {
        if (UnityEditor.EditorApplication.isPlaying == true) UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void PlayGame()
    {
        ResetLevel();

        // UI
        if (hudCanvas != null) hudCanvas.gameObject.SetActive(true);
        if (scoreValueDisplay != null) scoreValueDisplay.text = "" + score;
        if (livesValueDisplay != null) livesValueDisplay.text = "" + lives;

        // TIME
        if (timedLevel) currentTime = startTime;
        timerValueDisplay.text = "" + currentTime;

        gameStarted = true;
        SceneManager.LoadScene(gameLevel, LoadSceneMode.Additive);
        currentLevel = SceneManager.GetActiveScene().name;
    }

    public void BackToMenu()
    {
        playerIsDead = false;
        gameState = GameStates.Playing;

        HideMenus();
        menuCanvas.gameObject.SetActive(true);

        string levelToUnload = gameLevel;
        SceneManager.UnloadSceneAsync(levelToUnload);
    }

    public void ResetLevel()
    {
        playerIsDead = false;
        startTime = Time.time;
        gameState = GameStates.Playing;
        HideMenus();
        lives = defaultLives;
        score = defaultScore;
    }

    /**
     * Move the player to the next level
     */
    public void StartNextLevel()
    {
        backgroundMusicIsOver = false;
        lives = defaultLives;
        levelToLoad = gameLevel;
        string levelToUnload = currentLevel;

        PlayGame();
        SceneManager.UnloadSceneAsync(levelToUnload);
    }

    public void RestartGame()
    {
        score = defaultScore;
        lives = defaultLives;
        string levelToUnload = currentLevel;
        levelToLoad = menuLevel;
        PlayGame();
        SceneManager.UnloadSceneAsync(currentLevel);
    }

 //***********************************************
 // USER INTERFACE (UI)
 //***********************************************

    private void HideMenus()
    {
        menuCanvas.gameObject.SetActive(false);
        hudCanvas.gameObject.SetActive(false);
        endScreenCanvas.gameObject.SetActive(false);
        footerCanvas.gameObject.SetActive(false);
    }

    private void MainMenu()
    {
        defaultScore = 0;
        defaultLives = 3;
        gameTitle = "Game Title";
        gameCredits = "Made by NULLVALU_";
        gameCopyright = "Copyright " + year;

        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true);
        if (footerCanvas != null) footerCanvas.gameObject.SetActive(true);
    }

 //***********************************************
 // RUNTIME
 //***********************************************

    void Update()
    {
        // Check for Quit
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }

        if(Input.GetKeyDown(KeyCode.End))
        {
            gameState = GameStates.GameOver;
        }

        // Check for Debug
        if(Input.GetKeyDown(KeyCode.Y))
        {
            PrintGameState();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            lives--;
        }

        if (scoreValueDisplay.text != null) scoreValueDisplay.text = "" + score;
        if (livesValueDisplay.text != null) livesValueDisplay.text = "" + lives;

        switch(gameState)
        {
            case GameStates.Playing:
                // Lose Life Debug
                if (playerIsDead)
                {
                    if (lives > 0)
                    {
                        lives--;
                        ResetLevel();
                    }
                    else
                    {
                        gameState = GameStates.Death;
                    }
                }

                if(canBeatLevel && score >= beatLevelScore)
                {
                    gameState = GameStates.BeatLevel;
                }

                if(timedLevel)
                {
                    if(currentTime < 0)
                        gameState = GameStates.GameOver;
                    else
                    {
                        currentTime = Time.time - startTime;
                        timerValueDisplay.text = "" + currentTime.ToString("F2");
                    }
                }
                break;

            case GameStates.Death:
                if(backgroundMusic != null)
                {
                    backgroundMusic.volume -= 0.01f;
                    if(backgroundMusic.volume <= 0.0f)
                    {
                        AudioSource.PlayClipAtPoint(gameOverSFX, gameObject.transform.position);
                        endGameMessageDisplay.text = endMessageDisplayLose;
                        gameState = GameStates.GameOver;
                    }
                }
                break;

            case GameStates.BeatLevel:
                if(backgroundMusic != null)
                {
                    backgroundMusic.volume -= 1 * Time.deltaTime;
                    if (backgroundMusic.volume <= 0)
                    {
                        backgroundMusicIsOver = true;
                    }
                    if(backgroundMusicIsOver || backgroundMusic.clip == null)
                    {
                        AudioSource.PlayClipAtPoint(beatLevelSFX, gameObject.transform.position);
                        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
                        if(SceneManager.GetSceneAt(currentLevelIndex + 1) != null)
                        {
                            StartNextLevel();
                        }
                        else
                        {
                            endGameMessageDisplay.text = endMessageDisplayWin;
                            gameState = GameStates.GameOver;
                        }
                    }
                }
                break;

            case GameStates.GameOver:
                if(player) player.SetActive(false);

                HideMenus();
                if (endScreenCanvas)
                {
                    endScreenCanvas.gameObject.SetActive(true);
                }

                if(footerCanvas)
                {
                    footerCanvas.gameObject.SetActive(true);
                }
                break;
            default:
                break;
        }
    }

    private void PrintGameState()
    {
        Debug.Log("Player is dead: " + playerIsDead);
        Debug.Log("Game is over: " + isGameOver);
        Debug.Log("Beat level: " + beatLevelScore);
    }
}
