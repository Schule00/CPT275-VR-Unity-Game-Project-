using TMPro;
using UnityEngine;


public class Scoreboard : MonoBehaviour
{
    [Header("Settings")]
    public int scoreTotal = 0;
    public TextMeshProUGUI scoreText;

    public BowlingScoreboard _bowlingScoreboard;

    void Update()
    {
        if (_bowlingScoreboard.gameFinished)
        {
            scoreTotal += _bowlingScoreboard.BowlingScore;
            _bowlingScoreboard.gameFinished = false;
        }
        
        scoreText.text = scoreTotal.ToString() + " PTs";
    }
}
