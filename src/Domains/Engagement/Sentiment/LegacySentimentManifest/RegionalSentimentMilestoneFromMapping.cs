namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Functions of regional sentiment milestone from mapping.</summary>
public static class RegionalSentimentMilestoneFromMapping
{
    public static Dictionary<string, double> SummarizeRegionalTownHallMappingsForMilestone(List<RegionalTownHallMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<RegionalSentimentMilestone> DeriveRegionalSentimentMilestonesFromMapping(List<RegionalTownHallMapping> items)
    {
        var outValue = new List<RegionalSentimentMilestone>();
        foreach (var item in items)
        {
            var derived = RegionalSentimentMilestone.NewRegionalSentimentMilestone(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceRegionalSentimentMilestone(RegionalSentimentMilestone item)
    {
        return StrictTownHallLedgerPolicy.NormalizeStrictTownHallLedgerCode(item.Code);
    }
}
