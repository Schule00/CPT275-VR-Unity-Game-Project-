using System.Collections;
using TMPro;
using UnityEngine;

public class BowlingScoreboard : MonoBehaviour
{
    public static BowlingScoreboard Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    [Header("Settings")]
    public int totalPins = 5;
    public float celebrationDuration = 2f;

    [Header("Scoreboard")]
    public int BowlingScore;
    public bool gameFinished;

    private int _pinsDownThisThrow = 0;
    private int _totalPinsDown = 0;
    private int _throwCount = 0;
    private int _strikeCount = 0;

    private Coroutine _celebrationCoroutine;
    private BowlingPin[] _allPins;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _allPins = FindObjectsByType<BowlingPin>(FindObjectsSortMode.None);
    }

    private void Start() => RefreshDisplay();

    public void StartNewThrow()
    {
        _throwCount++;
        _pinsDownThisThrow = 0;
        gameFinished = false;
        foreach (var pin in _allPins)
            if (pin != null) pin.PrepareForNewThrow();
        if (_celebrationCoroutine != null) StopCoroutine(_celebrationCoroutine);
        RefreshDisplay();
    }

    public void RegisterPinDown()
    {
        _pinsDownThisThrow++;
        _totalPinsDown++;
        BowlingScore = _totalPinsDown * 10;
        gameFinished = true;
        if (_celebrationCoroutine != null) StopCoroutine(_celebrationCoroutine);
        _celebrationCoroutine = StartCoroutine(ShowCelebration());
    }

    public void ResetGame()
    {
        _pinsDownThisThrow = 0;
        _totalPinsDown = 0;
        _throwCount = 0;
        _strikeCount = 0;
        gameFinished = false;
        foreach (var pin in _allPins)
            if (pin != null) pin.ForceReset();
        if (_celebrationCoroutine != null) StopCoroutine(_celebrationCoroutine);
        RefreshDisplay();
    }

    private IEnumerator ShowCelebration()
    {
        if (_pinsDownThisThrow >= totalPins)
        {
            _strikeCount++;
            scoreText.text = "<color=#FFD700><b>STRIKE!</b></color>";
        }
        else if (_pinsDownThisThrow >= Mathf.CeilToInt(totalPins * 0.8f))
        {
            scoreText.text = "<color=#00CFFF><b>NICE!</b></color>";
        }
        else
        {
            RefreshDisplay();
            _celebrationCoroutine = null;
            yield break;
        }

        yield return new WaitForSeconds(celebrationDuration);
        RefreshDisplay();
        _celebrationCoroutine = null;
    }

    private void RefreshDisplay()
    {
        if (scoreText == null) return;
        scoreText.text =
            $"Throw {_throwCount}\n" +
            $"{_pinsDownThisThrow}/{totalPins} pins\n" +
            $"Total: {_totalPinsDown}  ⭐{_strikeCount}";
    }
}