using UnityEngine;

[System.Serializable]
public class BlackjackCard
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank { Ace=1, Two=2, Three=3, Four=4, Five=5,
                       Six=6, Seven=7, Eight=8, Nine=9, Ten=10,
                       Jack=11, Queen=12, King=13 }

    public Suit suit;
    public Rank rank;
    public bool isFaceUp = true;

    public BlackjackCard(Suit s, Rank r)
    {
        suit = s;
        rank = r;
    }

    // Blackjack value: face cards = 10, Ace = 1 or 11 (handled in Hand)
    public int BaseValue()
    {
        if (rank == Rank.Ace) return 11;
        if ((int)rank >= 10) return 10;
        return (int)rank;
    }

    public string GetSpriteName() => $"{rank}_of_{suit}".ToLower();
}
