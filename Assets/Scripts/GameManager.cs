using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject gameOverUI;          
    public MonoBehaviour playerController; 
    public Rigidbody2D playerRb;           

    private bool isGameOver;

    [Header("UI")]
    public TextMeshProUGUI survivalTimeText;  

    float runStartTime;

    void Awake()
    {
        
        Time.timeScale = 1f;
        runStartTime = Time.time;

        
        if (gameOverUI) gameOverUI.SetActive(false);
        isGameOver = false;
    }
    
    string FormatTime(float seconds)
{
    int m = (int)(seconds / 60f);
    int s = (int)(seconds % 60f);
    return $" - {m:00 minutes} : {s:00 seconds}";
}


    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;


        if (gameOverUI) gameOverUI.SetActive(true);


        if (playerController) playerController.enabled = false;
        if (playerRb) playerRb.velocity = Vector2.zero;

        Time.timeScale = 0f;

        float elapsed = Time.time - runStartTime;
        if (survivalTimeText != null) survivalTimeText.text = "You survived " + FormatTime(elapsed);


        Cursor.visible = true;
    }

    public void RestartGame()
    {
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        
        if (isGameOver && (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return)))
        {
            RestartGame();
        }
    }
}