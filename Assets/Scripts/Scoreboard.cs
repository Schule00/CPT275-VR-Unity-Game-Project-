using TMPro;
using UnityEngine;


public class Scoreboard : MonoBehaviour
{
    [Header("Settings")]
    public int scoreTotal = 0;
    public TextMeshProUGUI scoreText;

    public BowlingScoreboard _bowlingScoreboard;
    public SlotMachineUI _slotMachineUI;

    void Update()
    {
        
        if (_bowlingScoreboard.gameFinished)
        {
            scoreTotal += _bowlingScoreboard.BowlingScore;
            _bowlingScoreboard.gameFinished = false;
        }
        if (_slotMachineUI.gameFinished)
        {
            scoreTotal += _slotMachineUI.slotScore;
            _slotMachineUI.gameFinished = false;
        }
       
        scoreText.text = scoreTotal.ToString() + " PTs";
        
    }
}
