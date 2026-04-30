using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lightweight helper that loads the End Menu scene from anywhere in gameplay.
///
/// Two ways to use this:
///
/// 1. Drop this component onto a GameObject in the gameplay scene (e.g. a
///    "Game Over" trigger zone or a "Finish" button) and call EndGame() from
///    a UnityEvent in the Inspector.
///
/// 2. Call the static GameEndTrigger.End() from any other script:
///        GameEndTrigger.End();
///
/// Either path simply loads the End Menu scene; the EndMenuController then
/// pulls the final scores out of the GameManager and shows them.
/// </summary>
public class GameEndTrigger : MonoBehaviour
{
    [Tooltip("Name of the End Menu scene to load. Must match the scene file name and be in Build Settings.")]
    public string endMenuSceneName = "EndMenu";

    public void EndGame()
    {
        SceneManager.LoadScene(endMenuSceneName);
    }

    /// <summary>
    /// Static convenience for calling from non-MonoBehaviour code.
    /// Uses the default scene name "EndMenu".
    /// </summary>
    public static void End(string sceneName = "EndMenu")
    {
        SceneManager.LoadScene(sceneName);
    }
}
