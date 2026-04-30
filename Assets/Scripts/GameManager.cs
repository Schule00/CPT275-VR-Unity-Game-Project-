using UnityEngine;

/// <summary>
/// Persistent score / game-state holder. Survives scene loads via DontDestroyOnLoad
/// so the StartMenu, the VR Game scene, and the EndMenu can all read and write to
/// the same scores.
///
/// Hook into this from your gameplay scripts whenever a score event happens, e.g.:
///     GameManager.Instance.AddBowlingScore(1);
///     GameManager.Instance.AddSlotScore(50);
///
/// The StartMenu calls ResetScores() before loading the gameplay scene, and the
/// EndMenu reads BowlingScore / SlotScore to display the final results.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // --- Scores ---
    public int BowlingScore { get; private set; }
    public int SlotScore { get; private set; }

    private void Awake()
    {
        // Standard singleton + persistence pattern.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Score API (call these from gameplay scripts) ---

    public void AddBowlingScore(int amount)
    {
        BowlingScore += amount;
    }

    public void AddSlotScore(int amount)
    {
        SlotScore += amount;
    }

    public void SetBowlingScore(int value)
    {
        BowlingScore = value;
    }

    public void SetSlotScore(int value)
    {
        SlotScore = value;
    }

    /// <summary>
    /// Zeros out every score. Called when a new game begins (Start pressed on the
    /// StartMenu, or Restart pressed on the EndMenu).
    /// </summary>
    public void ResetScores()
    {
        BowlingScore = 0;
        SlotScore = 0;
    }
}
