namespace Acme.Workforce.Workplace.Visitor.Exception;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Functions of visitor budget from target.</summary>
public static class VisitorBudgetFromTarget
{
    public static Dictionary<string, double> SummarizeBatchedAccessBadgeTargetsForBudget(List<BatchedAccessBadgeTarget> items)
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

    public static List<VisitorBudget> DeriveVisitorBudgetsFromTarget(List<BatchedAccessBadgeTarget> items)
    {
        var outValue = new List<VisitorBudget>();
        foreach (var item in items)
        {
            var derived = VisitorBudget.NewVisitorBudget(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignVisitorBudget(VisitorBudget item)
    {
        return SeasonalAccessBadgeDigestPolicy.SeasonalAccessBadgeDigestBucket(item.MeasuredValue());
    }
}
