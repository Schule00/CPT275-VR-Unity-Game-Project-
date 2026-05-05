using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackjackUI : MonoBehaviour
{
    [Header("Score Display")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI dealerScoreText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI chipsText;
    public TextMeshProUGUI betText;

    [Header("Action Buttons")]
    public GameObject actionButtonPanel;
    public Button hitButton;
    public Button standButton;
    public Button doubleButton;

    [Header("Bet Buttons")]
    public GameObject betButtonPanel;
    public Button bet10Button;
    public Button bet25Button;
    public Button bet50Button;
    public Button bet100Button;

    private BlackjackGameManager gameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<BlackjackGameManager>();

        // Wire up action buttons
        hitButton.onClick.AddListener(() => gameManager.PlayerHit());
        standButton.onClick.AddListener(() => gameManager.PlayerStand());
        doubleButton.onClick.AddListener(() => gameManager.PlayerDoubleDown());

        // Wire up bet buttons
        bet10Button.onClick.AddListener(() => gameManager.PlaceBet(10));
        bet25Button.onClick.AddListener(() => gameManager.PlaceBet(25));
        bet50Button.onClick.AddListener(() => gameManager.PlaceBet(50));
        bet100Button.onClick.AddListener(() => gameManager.PlaceBet(100));

        ShowActionButtons(false);
        ShowBetButtons(false);
    }

    public void UpdateHands(int playerValue, string dealerValue)
    {
        if (playerScoreText != null)
            playerScoreText.text = $"Player: {playerValue}";
        if (dealerScoreText != null)
            dealerScoreText.text = $"Dealer: {dealerValue}";
    }

    public void UpdateState(string message, int chips, int bet)
    {
        if (message != null && messageText != null)
            messageText.text = message;
        if (chipsText != null)
            chipsText.text = $"Chips: ${chips}";
        if (betText != null && bet > 0)
            betText.text = $"Bet: ${bet}";
        else if (betText != null)
            betText.text = "";
    }

    public void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    public void ShowActionButtons(bool show)
    {
        if (actionButtonPanel != null)
            actionButtonPanel.SetActive(show);
    }

    public void ShowBetButtons(bool show)
    {
        if (betButtonPanel != null)
            betButtonPanel.SetActive(show);
    }
}
