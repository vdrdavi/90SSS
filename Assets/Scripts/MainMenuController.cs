using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public Animator transitionAnimator;
    public float transitionTime = 1f;
    public TextMeshProUGUI highScoreText;

    private void Start()
    {
        if (highScoreText != null)
        {
            float highScore = PlayerPrefs.GetFloat("HighScore", 0f);

            if (highScore > 0f)
            {
                highScoreText.text = "Highscore: " + highScore.ToString("F1") + "%";
                highScoreText.gameObject.SetActive(true);
            }
            else
            {
                highScoreText.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartCoroutine(LoadSceneRoutine("Game"));
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        transitionAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}