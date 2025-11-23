using System.Collections.Generic;
using System.Linq;

public static class HandEvaluator
{
    public enum HandRank
    {
        HighCard,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush
    }
    public class HandResult // 족보 평가 결과 반환용
    {
        public HandRank Rank;
        public List<int> Tiebreakers = new List<int>();// 족보 동일시 비교용
        public string Name;
        public bool UsedAllCards; // 완전한 족보면 true
    }

    public static HandResult EvaluateHand(List<Card> cards)
    {
        var result = new HandResult();
        // 족보별 필요한 카드 수랑 일치하면 UsedAllCards = true
        int nonNullCount = cards == null ? 0 : cards.Count(c => c != null);

        // Collect numbers and suits
        var numbers = cards.Where(c => c != null).Select(c => c.number).ToList(); // 1~13
        var numbersHigh = numbers.Select(n => n == 1 ? 14 : n).ToList(); // Ace 를 14로 변환한 버전
        numbersHigh.Sort();
        var suits = cards.Where(c => c != null).Select(c => c.suit).ToList();
        bool isFlush = suits.Count > 0 && suits.All(s => s == suits[0]); 

        int straightHigh = GetStraightHigh(numbers);
        bool isStraight = straightHigh > 0;

        // counts
        var counts = new Dictionary<int, int>();
        foreach (var n in numbersHigh)
        {
            counts[n] = counts.ContainsKey(n) ? counts[n] + 1 : 1;//
        }

        var groups = counts.GroupBy(kv => kv.Value)
                           .OrderByDescending(g => g.Key) // group by count: 4,3,2,1
                           .ToDictionary(g => g.Key, g => g.Select(p => p.Key).OrderByDescending(x => x).ToList());

        // Determine rank and tiebreakers
        if (isFlush && isStraight)
        {
            if (straightHigh == 14 && numbersHigh.Contains(10))
            {
                result.Rank = HandRank.RoyalFlush;
                result.Name = "Royal Flush";
            }
            else
            {
                result.Rank = HandRank.StraightFlush;
                result.Tiebreakers.Add(straightHigh);
                result.Name = "Straight Flush";
            }
            result.UsedAllCards = (nonNullCount == 5);
            return result;
        }

        if (groups.ContainsKey(4))
        {
            result.Rank = HandRank.FourOfAKind;
            int quad = groups[4][0];
            int kicker = counts.Where(kv => kv.Value == 1).Select(kv => kv.Key).OrderByDescending(x => x).FirstOrDefault();
            result.Tiebreakers.Add(quad);
            result.Tiebreakers.Add(kicker);
            result.Name = "Four of a Kind";
            result.UsedAllCards = (nonNullCount == 4);
            return result;
        }

        if (groups.ContainsKey(3) && groups.ContainsKey(2))
        {
            result.Rank = HandRank.FullHouse;
            int trip = groups[3][0];
            int pair = groups[2][0];
            result.Tiebreakers.Add(trip);
            result.Tiebreakers.Add(pair);
            result.Name = "Full House";
            result.UsedAllCards = (nonNullCount == 5);
            return result;
        }

        if (isFlush)
        {
            result.Rank = HandRank.Flush;
            var desc = numbersHigh.OrderByDescending(x => x).ToList();
            result.Tiebreakers.AddRange(desc);
            result.Name = "Flush";
            result.UsedAllCards = (nonNullCount == 5);
            return result;
        }

        if (isStraight)
        {
            result.Rank = HandRank.Straight;
            result.Tiebreakers.Add(straightHigh);
            result.Name = "Straight";
            result.UsedAllCards = (nonNullCount == 5);
            return result;
        }

        if (groups.ContainsKey(3))
        {
            result.Rank = HandRank.ThreeOfAKind;
            int trip = groups[3][0];
            var kickers = counts.Where(kv => kv.Value == 1).Select(kv => kv.Key).OrderByDescending(x => x).ToList();
            result.Tiebreakers.Add(trip);
            result.Tiebreakers.AddRange(kickers);
            result.Name = "Three of a Kind";
            result.UsedAllCards = (nonNullCount == 3);
            return result;
        }

        if (groups.ContainsKey(2) && groups[2].Count >= 2)
        {
            result.Rank = HandRank.TwoPair;
            var pairs = groups[2].OrderByDescending(x => x).ToList();
            int highPair = pairs[0];
            int lowPair = pairs[1];
            int kicker = counts.Where(kv => kv.Value == 1).Select(kv => kv.Key).OrderByDescending(x => x).FirstOrDefault();
            result.Tiebreakers.Add(highPair);
            result.Tiebreakers.Add(lowPair);
            result.Tiebreakers.Add(kicker);
            result.Name = "Two Pair";
            result.UsedAllCards = (nonNullCount == 4);
            return result;
        }

        if (groups.ContainsKey(2))
        {
            result.Rank = HandRank.OnePair;
            int pair = groups[2][0];
            var kickers = counts.Where(kv => kv.Value == 1).Select(kv => kv.Key).OrderByDescending(x => x).ToList();
            result.Tiebreakers.Add(pair);
            result.Tiebreakers.AddRange(kickers);
            result.Name = "One Pair";
            result.UsedAllCards = (nonNullCount == 2);
            return result;
        }

        // High Card
        result.Rank = HandRank.HighCard;
        var descHigh = numbersHigh.OrderByDescending(x => x).ToList();
        result.Tiebreakers.AddRange(descHigh);
        result.Name = "High Card";
        result.UsedAllCards = (nonNullCount == 1);
        return result;
    }

    static int GetStraightHigh(List<int> numbers) 
    {
        if (numbers == null || numbers.Count == 0) return 0;
        // normalized with Ace as 14
        var nums = numbers.Select(n => n == 1 ? 14 : n).Distinct().OrderBy(x => x).ToList();

        if (nums.Count < 5)
        {
            // check wheel A-2-3-4-5 using original numbers
            var raw = numbers.Distinct().OrderBy(x => x).ToList();
            if (raw.Contains(1) && raw.Contains(2) && raw.Contains(3) && raw.Contains(4) && raw.Contains(5)) return 5;
            return 0;
        }

        for (int i = 0; i <= nums.Count - 5; i++)
        {
            bool ok = true;
            for (int j = 0; j < 4; j++)
            {
                if (nums[i + j + 1] - nums[i + j] != 1) { ok = false; break; }
            }
            if (ok) return nums[i + 4];
        }

        // final check for wheel (A as 1)
        var rawNums = numbers.Distinct().OrderBy(x => x).ToList();
        if (rawNums.Contains(1) && rawNums.Contains(2) && rawNums.Contains(3) && rawNums.Contains(4) && rawNums.Contains(5)) return 5;
        return 0;
    }
}
