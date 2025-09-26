using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject gameOverUI;          
    public MonoBehaviour playerController; 
    public Rigidbody2D playerRb;           

    private bool isGameOver;

    void Awake()
    {
        
        Time.timeScale = 1f;

        
        if (gameOverUI) gameOverUI.SetActive(false);
        isGameOver = false;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        
        if (gameOverUI) gameOverUI.SetActive(true);

        
        if (playerController) playerController.enabled = false;
        if (playerRb) playerRb.velocity = Vector2.zero;     
        
        Time.timeScale = 0f;

        
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