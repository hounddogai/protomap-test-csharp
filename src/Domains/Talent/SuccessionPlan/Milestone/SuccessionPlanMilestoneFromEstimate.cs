namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.Pool.Estimate;

/// <summary>Functions of succession plan milestone from estimate.</summary>
public static class SuccessionPlanMilestoneFromEstimate
{
    public static Dictionary<string, double> SummarizePoolEstimatesForMilestone(List<PoolEstimate> items)
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

    public static List<SuccessionPlanMilestone> DeriveSuccessionPlanMilestonesFromEstimate(List<PoolEstimate> items)
    {
        var outValue = new List<SuccessionPlanMilestone>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanMilestone.NewSuccessionPlanMilestone(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileSuccessionPlanMilestone(SuccessionPlanMilestone item)
    {
        return PoolEntryPolicy.NormalizePoolEntryCode(item.Code);
    }
}
