using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class StartMenuController : MonoBehaviour
{
    [Header("UI Buttons")]
    [Tooltip("Button that begins the game.")]
    public Button startButton;

    [Tooltip("Button that quits the application.")]
    public Button cancelButton;

    [Header("Scene")]
    [Tooltip("Name of the gameplay scene to load when Start is pressed. " +
             "Must match the scene file name in Assets/Scenes and be added to Build Settings.")]
    public string gameplaySceneName = "VR Game";

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
    }

    private void OnEnable()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartPressed);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelPressed);
    }

    private void OnDisable()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartPressed);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(OnCancelPressed);
    }

    /// <summary>
    /// Public so it can also be called directly from a UnityEvent (e.g. an
    /// XR Simple Interactable's "Select Entered" event) without going through
    /// a Button component, which is useful for VR diegetic buttons.
    /// </summary>
    public void OnStartPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResetScores();

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnCancelPressed()
    {
#if UNITY_EDITOR
        // In the editor, Application.Quit() does nothing, so we stop Play Mode instead.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
