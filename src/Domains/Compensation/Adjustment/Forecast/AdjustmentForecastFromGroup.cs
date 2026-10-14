namespace Acme.Workforce.Compensation.Adjustment.Forecast;

using Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>Functions of adjustment forecast from group.</summary>
public static class AdjustmentForecastFromGroup
{
    public static Dictionary<string, double> SummarizeManualEquityGrantGroupsForForecast(List<ManualEquityGrantGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<AdjustmentForecast> DeriveAdjustmentForecastsFromGroup(List<ManualEquityGrantGroup> items)
    {
        var outValue = new List<AdjustmentForecast>();
        foreach (var item in items)
        {
            var derived = AdjustmentForecast.NewAdjustmentForecast(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankAdjustmentForecast(AdjustmentForecast item)
    {
        return EquityGrantProposalPolicy.ClassifyEquityGrantProposal(item.ProjectedLoad());
    }
}
