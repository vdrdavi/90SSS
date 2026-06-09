using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject instructionsPanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject hudPanel;
    public TextMeshProUGUI timerText;
    public AudioSource backgroundMusic;

    public float timeLimit = 60f;
    private float currentTime;
    private bool isGameActive = false;

    private void Start()
    {
        Time.timeScale = 0f;
        instructionsPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        currentTime = timeLimit;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!isGameActive && instructionsPanel.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            StartGame();
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
            timerText.text = Mathf.Max(0, seconds).ToString() + "s";
        }
    }

    public void TriggerGameOver()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);

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

        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }
    }
}