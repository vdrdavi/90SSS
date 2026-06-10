using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject instructionsPanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject hudPanel;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI victoryStatsText;
    public AudioSource backgroundMusic;
    public string mainMenuSceneName = "MainMenu";

    public float timeLimit = 60f;
    private float currentTime;
    private bool isGameActive = false;

    public List<ItemData> allSpawnedItems = new List<ItemData>();

    private void Start()
    {
        Time.timeScale = 0f;
        instructionsPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        currentTime = timeLimit;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!isGameActive)
        {
            if (instructionsPanel.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                StartGame();
            }
            else if (gameOverPanel.activeSelf || victoryPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(mainMenuSceneName);
                }
            }
        }

        if (isGameActive)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            if (currentTime <= 0)
            {
                TriggerGameOver();
            }
        }
    }

    private void StartGame()
    {
        isGameActive = true;
        instructionsPanel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }

        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(currentTime);
            timerText.text = "Tempo: " + Mathf.Max(0, seconds).ToString() + "s";
        }
    }

    public void TriggerGameOver()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (hudPanel != null) hudPanel.SetActive(false);

        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }
    }

    public void TriggerVictory()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        victoryPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (hudPanel != null) hudPanel.SetActive(false);

        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        CalculateFinalScore();
    }

    private void CalculateFinalScore()
    {
        if (victoryStatsText == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Inventory playerInventory = player.GetComponent<Inventory>();
        int playerValue = playerInventory.totalValue;
        int maxCapacity = playerInventory.maxWeight;

        int optimalValue = CalculateKnapsack(maxCapacity);

        float efficiency = optimalValue > 0 ? ((float)playerValue / optimalValue) * 100f : 0f;

        float savedHighScore = PlayerPrefs.GetFloat("HighScore", 0f);
        bool isNewRecord = false;

        if (efficiency > savedHighScore)
        {
            PlayerPrefs.SetFloat("HighScore", efficiency);
            PlayerPrefs.Save();
            isNewRecord = true;
        }

        victoryStatsText.text = $"Valor Roubado: ${playerValue}\n" +
                                $"Valor Perfeito Possível: ${optimalValue}\n" +
                                $"Eficiência: {efficiency:F1}%\n\n" +
                                (isNewRecord ? "<color=yellow>NOVO RECORDE!</color>" : $"Melhor Marca: {Mathf.Max(efficiency, savedHighScore):F1}%");
    }

    private int CalculateKnapsack(int maxWeight)
    {
        int n = allSpawnedItems.Count;
        int[,] dp = new int[n + 1, maxWeight + 1];

        for (int i = 0; i <= n; i++)
        {
            for (int w = 0; w <= maxWeight; w++)
            {
                if (i == 0 || w == 0)
                {
                    dp[i, w] = 0;
                }
                else if (allSpawnedItems[i - 1].weight <= w)
                {
                    dp[i, w] = Mathf.Max(
                        allSpawnedItems[i - 1].value + dp[i - 1, w - allSpawnedItems[i - 1].weight],
                        dp[i - 1, w]
                    );
                }
                else
                {
                    dp[i, w] = dp[i - 1, w];
                }
            }
        }

        return dp[n, maxWeight];
    }
}