namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Functions of manual assignment rule from forecast.</summary>
public static class ManualAssignmentRuleFromForecast
{
    public static Dictionary<string, double> SummarizeRollingRefreshForecastsForRule(List<RollingRefreshForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<ManualAssignmentRule> DeriveManualAssignmentRulesFromForecast(List<RollingRefreshForecast> items)
    {
        var outValue = new List<ManualAssignmentRule>();
        foreach (var item in items)
        {
            var derived = ManualAssignmentRule.NewManualAssignmentRule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyManualAssignmentRule(ManualAssignmentRule item)
    {
        return RefreshCheckpointPolicy.RefreshCheckpointWeight(1, true);
    }
}
