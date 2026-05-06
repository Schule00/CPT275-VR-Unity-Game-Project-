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
        scoreTotal += _bowlingScoreboard.BowlingScore;
        scoreText.text = scoreTotal.ToString() + " PTs";
    }
}
