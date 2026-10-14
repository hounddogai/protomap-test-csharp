namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Absence.Accrual.Batch;

/// <summary>Functions of balance milestone from estimate.</summary>
public static class BalanceMilestoneFromEstimate
{
    public static Dictionary<string, double> SummarizeAccrualEstimatesForMilestone(List<AccrualEstimate> items)
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

    public static List<BalanceMilestone> DeriveBalanceMilestonesFromEstimate(List<AccrualEstimate> items)
    {
        var outValue = new List<BalanceMilestone>();
        foreach (var item in items)
        {
            var derived = BalanceMilestone.NewBalanceMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreBalanceMilestone(BalanceMilestone item)
    {
        return AccrualBatchPolicy.AccrualBatchBucket(item.ProjectedLoad());
    }
}
