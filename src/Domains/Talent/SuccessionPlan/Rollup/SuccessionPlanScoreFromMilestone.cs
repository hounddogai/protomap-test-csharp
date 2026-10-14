namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Functions of succession plan score from milestone.</summary>
public static class SuccessionPlanScoreFromMilestone
{
    public static Dictionary<string, double> SummarizeSuccessionPlanMilestonesForScore(List<SuccessionPlanMilestone> items)
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

    public static List<SuccessionPlanScore> DeriveSuccessionPlanScoresFromMilestone(List<SuccessionPlanMilestone> items)
    {
        var outValue = new List<SuccessionPlanScore>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanScore.NewSuccessionPlanScore(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanSuccessionPlanScore(SuccessionPlanScore item)
    {
        return SuccessionPlanMilestonePolicy.SuccessionPlanMilestoneBucket(item.NetLoad());
    }
}
