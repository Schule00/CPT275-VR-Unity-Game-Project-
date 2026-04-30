using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Scoreboard manager. Attach to an empty GameObject called "ScoreboardManager".
/// Drag your World Space Canvas TextMeshPro into the Score Text field.
/// </summary>
public class BowlingScoreboard : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────
    public static BowlingScoreboard Instance { get; private set; }

    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("UI Reference")]
    public TextMeshProUGUI scoreText;

    [Header("Game Settings")]
    public int totalPins = 5;
    public float celebrationDuration = 2f;

    // ── State ─────────────────────────────────────────────────────────────────
    private int _pinsDownThisThrow = 0;
    private int _totalPinsDown = 0;
    private int _throwCount = 0;

    private Coroutine _celebrationCoroutine;
    private BowlingPin[] _allPins;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _allPins = FindObjectsByType<BowlingPin>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        RefreshDisplay();
    }

    // ── Public API — called by BowlingBall ────────────────────────────────────
    /// <summary>
    /// Called once per throw, when the ball reaches throw speed after release.
    /// Increments the throw counter, resets this-throw pin count,
    /// and unlocks every pin to be counted once this throw.
    /// </summary>
    public void StartNewThrow()
    {
        _throwCount++;
        _pinsDownThisThrow = 0;

        foreach (var pin in _allPins)
            if (pin != null) pin.PrepareForNewThrow();

        if (_celebrationCoroutine != null)
            StopCoroutine(_celebrationCoroutine);

        RefreshDisplay();
    }

    // ── Public API — called by BowlingPin ────────────────────────────────────
    public void RegisterPinDown()
    {
        _pinsDownThisThrow++;
        _totalPinsDown++;

        if (_celebrationCoroutine != null)
            StopCoroutine(_celebrationCoroutine);

        _celebrationCoroutine = StartCoroutine(ShowScoreWithCelebration());
    }

    // ── Full game reset ───────────────────────────────────────────────────────
    public void ResetGame()
    {
        _pinsDownThisThrow = 0;
        _totalPinsDown = 0;
        _throwCount = 0;

        foreach (var pin in _allPins)
            if (pin != null) pin.PrepareForNewThrow();

        if (_celebrationCoroutine != null)
            StopCoroutine(_celebrationCoroutine);

        RefreshDisplay();
    }

    // ── Display ───────────────────────────────────────────────────────────────
    private IEnumerator ShowScoreWithCelebration()
    {
        string msg = GetCelebrationMessage();

        if (!string.IsNullOrEmpty(msg))
        {
            SetText(msg);
            yield return new WaitForSeconds(celebrationDuration);
        }

        RefreshDisplay();
        _celebrationCoroutine = null;
    }

    private void RefreshDisplay()
    {
        if (scoreText == null) return;

        scoreText.text =
           
            $"<b>Throw:</b>  {_throwCount}\n" +
            $"<b>This throw:</b>  {_pinsDownThisThrow} / {totalPins}\n" +
            $"<b>Total pins:</b>  {_totalPinsDown}";
    }

    private void SetText(string msg)
    {
        if (scoreText != null) scoreText.text = msg;
    }

    private string GetCelebrationMessage()
    {
        if (_pinsDownThisThrow >= totalPins)
            return "<color=#FFD700><size=120%><b>STRIKE!</b></size></color>";

        if (_pinsDownThisThrow >= Mathf.CeilToInt(totalPins * 0.8f))
            return "<color=#00CFFF><size=110%><b>NICE SHOT!</b></size></color>";

        return string.Empty;
    }
}