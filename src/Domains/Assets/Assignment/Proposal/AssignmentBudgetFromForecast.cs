namespace Acme.Workforce.Assets.Assignment.Proposal;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Functions of assignment budget from forecast.</summary>
public static class AssignmentBudgetFromForecast
{
    public static Dictionary<string, double> SummarizeRefreshForecastsForBudget(List<RefreshForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<AssignmentBudget> DeriveAssignmentBudgetsFromForecast(List<RefreshForecast> items)
    {
        var outValue = new List<AssignmentBudget>();
        foreach (var item in items)
        {
            var derived = AssignmentBudget.NewAssignmentBudget(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAssignmentBudget(AssignmentBudget item)
    {
        return RefreshSegmentPolicy.RefreshSegmentBucket(item.AdjustedExposure());
    }
}
