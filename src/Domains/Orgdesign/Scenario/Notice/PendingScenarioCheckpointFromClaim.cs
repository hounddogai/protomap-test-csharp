namespace Acme.Workforce.Orgdesign.Scenario.Notice;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Functions of pending scenario checkpoint from claim.</summary>
public static class PendingScenarioCheckpointFromClaim
{
    public static Dictionary<string, double> SummarizeTieredHeadcountClaimsForCheckpoint(List<TieredHeadcountClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PendingScenarioCheckpoint> DerivePendingScenarioCheckpointsFromClaim(List<TieredHeadcountClaim> items)
    {
        var outValue = new List<PendingScenarioCheckpoint>();
        foreach (var item in items)
        {
            var derived = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPendingScenarioCheckpoint(PendingScenarioCheckpoint item)
    {
        return HeadcountForecastPolicy.NormalizeHeadcountForecastCode(item.Code);
    }
}
