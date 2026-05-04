using System.Collections.Generic;
using UnityEngine;

public class BlackjackDeck : MonoBehaviour
{
    private List<BlackjackCard> cards = new List<BlackjackCard>();

    public void Initialize(int numDecks = 1)
    {
        cards.Clear();
        for (int d = 0; d < numDecks; d++)
            foreach (BlackjackCard.Suit s in System.Enum.GetValues(typeof(BlackjackCard.Suit)))
                foreach (BlackjackCard.Rank r in System.Enum.GetValues(typeof(BlackjackCard.Rank)))
                    cards.Add(new BlackjackCard(s, r));
        Shuffle();
    }

    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }

    public BlackjackCard DrawCard()
    {
        if (cards.Count == 0)
        {
            Debug.LogWarning("Deck empty — reshuffling!");
            Initialize();
        }
        BlackjackCard card = cards[0];
        cards.RemoveAt(0);
        return card;
    }

    public int CardsRemaining => cards.Count;
}
