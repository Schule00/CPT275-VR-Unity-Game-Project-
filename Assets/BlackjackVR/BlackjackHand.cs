using System.Collections.Generic;

public class BlackjackHand
{
    public List<BlackjackCard> Cards { get; private set; } = new List<BlackjackCard>();

    public void AddCard(BlackjackCard card) => Cards.Add(card);
    public void Clear() => Cards.Clear();

    public int GetValue()
    {
        int total = 0;
        int aces = 0;

        foreach (var card in Cards)
        {
            if (!card.isFaceUp) continue; // Don't count hidden dealer card
            int val = card.BaseValue();
            if (card.rank == BlackjackCard.Rank.Ace) aces++;
            total += val;
        }

        // Reduce Aces from 11 to 1 if bust
        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        return total;
    }

    public bool IsBust() => GetValue() > 21;
    public bool IsBlackjack() => Cards.Count == 2 && GetValue() == 21;

    // Returns true if hand contains an Ace counted as 11
    public bool IsSoft()
    {
        int total = 0;
        int aces = 0;
        foreach (var card in Cards)
        {
            total += card.BaseValue();
            if (card.rank == BlackjackCard.Rank.Ace) aces++;
        }
        return aces > 0 && total <= 21;
    }
}
