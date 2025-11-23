using UnityEngine;
using System;

public class Card
{
    public enum Suit
    {
        Spade,
        Heart,
        Diamond,
        Club
    }

    public Suit suit;
    public int number;        // 1~13

    public Card(Suit suit, int number)
    {
        this.suit = suit;
        this.number = number;
    }   

    // Parse a sprite filename like "spade_1" (case-insensitive) into a Card instance.
    public static bool TryParseSpriteNameToCard(string spriteName, out Card card)
    {
        card = null;
        if (string.IsNullOrEmpty(spriteName)) return false;
        var name = spriteName.ToLower();
        var parts = name.Split('_');
        if (parts.Length < 2) return false;
        if (!int.TryParse(parts[1], out int num)) return false;
        try
        {
            var suit = (Suit)System.Enum.Parse(typeof(Suit), parts[0], true);
            card = new Card(suit, num);
            return true;
        }
        catch { return false; }
    }
}