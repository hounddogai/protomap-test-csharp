namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Functions of regional access badge score from manifest.</summary>
public static class RegionalAccessBadgeScoreFromManifest
{
    public static Dictionary<string, double> SummarizeCappedAccessBadgeManifestsForScore(List<CappedAccessBadgeManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<RegionalAccessBadgeScore> DeriveRegionalAccessBadgeScoresFromManifest(List<CappedAccessBadgeManifest> items)
    {
        var outValue = new List<RegionalAccessBadgeScore>();
        foreach (var item in items)
        {
            var derived = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRegionalAccessBadgeScore(RegionalAccessBadgeScore item)
    {
        return CappedAccessBadgeMappingPolicy.CappedAccessBadgeMappingBucket(item.NetValue());
    }
}
