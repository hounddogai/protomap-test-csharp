namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Functions of scheduled pool milestone from audit.</summary>
public static class ScheduledPoolMilestoneFromAudit
{
    public static Dictionary<string, double> SummarizeScheduledSuccessionPlanAuditsForMilestone(List<ScheduledSuccessionPlanAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<ScheduledPoolMilestone> DeriveScheduledPoolMilestonesFromAudit(List<ScheduledSuccessionPlanAudit> items)
    {
        var outValue = new List<ScheduledPoolMilestone>();
        foreach (var item in items)
        {
            var derived = ScheduledPoolMilestone.NewScheduledPoolMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyScheduledPoolMilestone(ScheduledPoolMilestone item)
    {
        return SuccessionPlanAuditPolicy.ClassifySuccessionPlanAudit(item.ProjectedTotal());
    }
}
