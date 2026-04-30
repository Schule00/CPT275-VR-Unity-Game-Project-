using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndMenuController : MonoBehaviour
{
    [Header("Score Display (assign one of each pair)")]
    [Tooltip("TextMeshPro field that shows the final bowling score. Optional if using legacyBowlingText.")]
    public TextMeshProUGUI bowlingScoreText;

    [Tooltip("TextMeshPro field that shows the final slot machine score. Optional if using legacySlotText.")]
    public TextMeshProUGUI slotScoreText;

    [Header("UI Buttons")]
    public Button restartButton;
    public Button exitButton;

    [Header("Scenes")]
    [Tooltip("Name of the gameplay scene to load when Restart is pressed.")]
    public string gameplaySceneName = "VR Game";

    [Header("Display Format")]
    [Tooltip("Format string for the bowling score. {0} is replaced with the score.")]
    public string bowlingFormat = "Bowling Score: {0}";

    [Tooltip("Format string for the slot score. {0} is replaced with the score.")]
    public string slotFormat = "Slot Machine Score: {0}";

    private void OnEnable()
    {
        UpdateScoreDisplay();

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartPressed);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitPressed);
    }

    private void OnDisable()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartPressed);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitPressed);
    }

    private void UpdateScoreDisplay()
    {
        int bowling = GameManager.Instance != null ? GameManager.Instance.BowlingScore : 0;
        int slot    = GameManager.Instance != null ? GameManager.Instance.SlotScore    : 0;

        string bowlingDisplay = string.Format(bowlingFormat, bowling);
        string slotDisplay    = string.Format(slotFormat,    slot);

        if (bowlingScoreText != null) bowlingScoreText.text = bowlingDisplay;

        if (slotScoreText != null) slotScoreText.text = slotDisplay;
    }

    public void OnRestartPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResetScores();

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnExitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
