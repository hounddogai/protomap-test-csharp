namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Compensation.EquityGrant.Agreement;

/// <summary>Functions of regional merit cycle bundle from summary.</summary>
public static class RegionalMeritCycleBundleFromSummary
{
    public static Dictionary<string, double> SummarizePrimaryEquityGrantSummariesForBundle(List<PrimaryEquityGrantSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<RegionalMeritCycleBundle> DeriveRegionalMeritCycleBundlesFromSummary(List<PrimaryEquityGrantSummary> items)
    {
        var outValue = new List<RegionalMeritCycleBundle>();
        foreach (var item in items)
        {
            var derived = RegionalMeritCycleBundle.NewRegionalMeritCycleBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRegionalMeritCycleBundle(RegionalMeritCycleBundle item)
    {
        return PrimaryEquityGrantSummaryPolicy.ClassifyPrimaryEquityGrantSummary(item.AdjustedTotal());
    }
}
