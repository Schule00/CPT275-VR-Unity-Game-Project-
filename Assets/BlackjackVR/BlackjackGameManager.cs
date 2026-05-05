using System.Collections;
using UnityEngine;

public class BlackjackGameManager : MonoBehaviour
{
    public enum GameState { WaitingForBet, PlayerTurn, DealerTurn, RoundOver }

    [Header("References")]
    public BlackjackDeck deck;
    public BlackjackUI ui;
    public CardLayoutManager cardLayout;

    [Header("Settings")]
    public int startingChips = 500;
    public int minBet = 10;
    public int maxBet = 500;

    private BlackjackHand playerHand = new BlackjackHand();
    private BlackjackHand dealerHand = new BlackjackHand();
    private GameState currentState;
    private int playerChips;
    private int currentBet;

    void Start()
    {
        playerChips = startingChips;
        deck.Initialize(numDecks: 6); // Casino standard: 6 decks
        StartNewRound();
    }

    public void StartNewRound()
    {
        playerHand.Clear();
        dealerHand.Clear();
        cardLayout.ClearTable();

        currentState = GameState.WaitingForBet;
        ui.UpdateState("Place your bet!", playerChips, 0);
        ui.ShowBetButtons(true);
    }

    public void PlaceBet(int amount)
    {
        if (amount > playerChips || amount < minBet) return;
        currentBet = amount;
        playerChips -= amount;
        StartCoroutine(DealInitialCards());
    }

    IEnumerator DealInitialCards()
    {
        ui.ShowBetButtons(false);
        currentState = GameState.PlayerTurn;

        // Classic deal order: Player, Dealer, Player, Dealer (face down)
        yield return DealCardTo(playerHand, isPlayer: true, faceUp: true);
        yield return new WaitForSeconds(0.4f);
        yield return DealCardTo(dealerHand, isPlayer: false, faceUp: true);
        yield return new WaitForSeconds(0.4f);
        yield return DealCardTo(playerHand, isPlayer: true, faceUp: true);
        yield return new WaitForSeconds(0.4f);
        yield return DealCardTo(dealerHand, isPlayer: false, faceUp: false); // Hole card
        yield return new WaitForSeconds(0.4f);

        ui.UpdateHands(playerHand.GetValue(), "?");
        ui.ShowActionButtons(true);

        if (playerHand.IsBlackjack())
        {
            ui.ShowMessage("Blackjack!");
            yield return new WaitForSeconds(1f);
            StartCoroutine(DealerTurn());
        }
    }

    IEnumerator DealCardTo(BlackjackHand hand, bool isPlayer, bool faceUp)
    {
        BlackjackCard card = deck.DrawCard();
        card.isFaceUp = faceUp;
        hand.AddCard(card);
        cardLayout.PlaceCard(card, isPlayer);
        yield return new WaitForSeconds(0.3f);
    }

    public void PlayerHit()
    {
        if (currentState != GameState.PlayerTurn) return;
        StartCoroutine(PlayerHitRoutine());
    }

    IEnumerator PlayerHitRoutine()
    {
        ui.ShowActionButtons(false);
        yield return DealCardTo(playerHand, isPlayer: true, faceUp: true);
        ui.UpdateHands(playerHand.GetValue(), "?");

        if (playerHand.IsBust())
        {
            ui.ShowMessage("Bust! You lose.");
            yield return new WaitForSeconds(1.5f);
            EndRound(playerWon: false, push: false);
        }
        else
        {
            ui.ShowActionButtons(true);
        }
    }

    public void PlayerStand()
    {
        if (currentState != GameState.PlayerTurn) return;
        ui.ShowActionButtons(false);
        StartCoroutine(DealerTurn());
    }

    public void PlayerDoubleDown()
    {
        if (currentState != GameState.PlayerTurn) return;
        if (playerChips < currentBet) return; // Can't afford double
        if (playerHand.Cards.Count != 2) return; // Only on first two cards

        playerChips -= currentBet;
        currentBet *= 2;
        ui.UpdateState(null, playerChips, currentBet);

        StartCoroutine(DoubleDownRoutine());
    }

    IEnumerator DoubleDownRoutine()
    {
        ui.ShowActionButtons(false);
        yield return DealCardTo(playerHand, isPlayer: true, faceUp: true);
        ui.UpdateHands(playerHand.GetValue(), "?");
        yield return new WaitForSeconds(0.5f);

        if (playerHand.IsBust())
        {
            ui.ShowMessage("Bust! You lose.");
            yield return new WaitForSeconds(1.5f);
            EndRound(playerWon: false, push: false);
        }
        else
        {
            StartCoroutine(DealerTurn());
        }
    }

    IEnumerator DealerTurn()
    {
        currentState = GameState.DealerTurn;

        // Flip hole card
        RevealDealerHoleCard();
        yield return new WaitForSeconds(0.5f);
        ui.UpdateHands(playerHand.GetValue(), dealerHand.GetValue().ToString());

        // Dealer hits on soft 17 (standard casino rule)
        while (dealerHand.GetValue() < 17 || (dealerHand.GetValue() == 17 && dealerHand.IsSoft()))
        {
            yield return new WaitForSeconds(0.8f);
            yield return DealCardTo(dealerHand, isPlayer: false, faceUp: true);
            ui.UpdateHands(playerHand.GetValue(), dealerHand.GetValue().ToString());
        }

        yield return new WaitForSeconds(0.5f);
        ResolveRound();
    }

    void RevealDealerHoleCard()
    {
        foreach (var card in dealerHand.Cards)
            card.isFaceUp = true;
        cardLayout.FlipAllDealerCards();
    }

    void ResolveRound()
    {
        int playerVal = playerHand.GetValue();
        int dealerVal = dealerHand.GetValue();
        bool playerBJ = playerHand.IsBlackjack();
        bool dealerBJ = dealerHand.IsBlackjack();

        if (dealerBJ && playerBJ)
        {
            ui.ShowMessage("Push — both Blackjack!");
            EndRound(playerWon: false, push: true);
        }
        else if (playerBJ)
        {
            ui.ShowMessage("Blackjack! You win 3:2!");
            playerChips += Mathf.RoundToInt(currentBet * 2.5f);
            EndRound(playerWon: true, push: false, skipChips: true);
        }
        else if (dealerBJ || (!playerBJ && dealerVal > playerVal && !dealerHand.IsBust()))
        {
            ui.ShowMessage("Dealer wins.");
            EndRound(playerWon: false, push: false);
        }
        else if (dealerHand.IsBust() || playerVal > dealerVal)
        {
            ui.ShowMessage("You win!");
            EndRound(playerWon: true, push: false);
        }
        else if (playerVal == dealerVal)
        {
            ui.ShowMessage("Push!");
            EndRound(playerWon: false, push: true);
        }
        else
        {
            ui.ShowMessage("Dealer wins.");
            EndRound(playerWon: false, push: false);
        }
    }

    void EndRound(bool playerWon, bool push, bool skipChips = false)
    {
        currentState = GameState.RoundOver;
        if (!skipChips)
        {
            if (playerWon) playerChips += currentBet * 2;
            else if (push) playerChips += currentBet;
        }
        ui.UpdateState(null, playerChips, 0);
        Invoke(nameof(StartNewRound), 3f);
    }
}
