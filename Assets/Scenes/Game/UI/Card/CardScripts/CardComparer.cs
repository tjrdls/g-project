using System.Collections.Generic;
using System.Linq;

// A가 B보다 크면 1, 작으면 -1, 같으면 0 반환
public static class CardComparer
{
    public static int Compare(HandEvaluator.HandResult aInfo, HandEvaluator.HandResult bInfo)
    {
        if (aInfo == null && bInfo == null) return 0;
        if (aInfo == null) return -1;
        if (bInfo == null) return 1;

        if (aInfo.Rank > bInfo.Rank) return 1;
        if (aInfo.Rank < bInfo.Rank) return -1;

        var av = aInfo.Tiebreakers ?? new List<int>();
        var bv = bInfo.Tiebreakers ?? new List<int>();
        int n = System.Math.Max(av.Count, bv.Count);
        for (int i = 0; i < n; i++)
        {
            int ai = i < av.Count ? av[i] : 0;
            int bi = i < bv.Count ? bv[i] : 0;
            if (ai > bi) return 1;
            if (ai < bi) return -1;
        }

        return 0;
    }
}
