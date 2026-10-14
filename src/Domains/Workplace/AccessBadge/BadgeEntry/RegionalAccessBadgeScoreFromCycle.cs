namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Functions of regional access badge score from cycle.</summary>
public static class RegionalAccessBadgeScoreFromCycle
{
    public static Dictionary<string, double> SummarizeRegionalDeskCyclesForScore(List<RegionalDeskCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<RegionalAccessBadgeScore> DeriveRegionalAccessBadgeScoresFromCycle(List<RegionalDeskCycle> items)
    {
        var outValue = new List<RegionalAccessBadgeScore>();
        foreach (var item in items)
        {
            var derived = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateRegionalAccessBadgeScore(RegionalAccessBadgeScore item)
    {
        return FallbackDeskSnapshotPolicy.NormalizeFallbackDeskSnapshotCode(item.Code);
    }
}
