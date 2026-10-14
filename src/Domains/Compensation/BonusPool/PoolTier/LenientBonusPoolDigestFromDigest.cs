namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Functions of lenient bonus pool digest from digest.</summary>
public static class LenientBonusPoolDigestFromDigest
{
    public static Dictionary<string, double> SummarizeBonusPoolDigestsForDigest(List<BonusPoolDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<LenientBonusPoolDigest> DeriveLenientBonusPoolDigestsFromDigest(List<BonusPoolDigest> items)
    {
        var outValue = new List<LenientBonusPoolDigest>();
        foreach (var item in items)
        {
            var derived = LenientBonusPoolDigest.NewLenientBonusPoolDigest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLenientBonusPoolDigest(LenientBonusPoolDigest item)
    {
        return BonusPoolReviewPolicy.NormalizeBonusPoolReviewCode(item.Code);
    }
}
