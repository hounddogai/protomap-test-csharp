namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Functions of weighted statement of work bundle from target.</summary>
public static class WeightedStatementOfWorkBundleFromTarget
{
    public static Dictionary<string, double> SummarizeEngagementTargetsForBundle(List<EngagementTarget> items)
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

    public static List<WeightedStatementOfWorkBundle> DeriveWeightedStatementOfWorkBundlesFromTarget(List<EngagementTarget> items)
    {
        var outValue = new List<WeightedStatementOfWorkBundle>();
        foreach (var item in items)
        {
            var derived = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleWeightedStatementOfWorkBundle(WeightedStatementOfWorkBundle item)
    {
        return EngagementTargetPolicy.EngagementTargetWeight(7, true);
    }
}
