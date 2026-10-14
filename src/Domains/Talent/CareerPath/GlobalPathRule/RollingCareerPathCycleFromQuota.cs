namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>Functions of rolling career path cycle from quota.</summary>
public static class RollingCareerPathCycleFromQuota
{
    public static Dictionary<string, double> SummarizeFallbackSuccessionPlanQuotasForCycle(List<FallbackSuccessionPlanQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<RollingCareerPathCycle> DeriveRollingCareerPathCyclesFromQuota(List<FallbackSuccessionPlanQuota> items)
    {
        var outValue = new List<RollingCareerPathCycle>();
        foreach (var item in items)
        {
            var derived = RollingCareerPathCycle.NewRollingCareerPathCycle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreRollingCareerPathCycle(RollingCareerPathCycle item)
    {
        return RollingSuccessionPlanRequestPolicy.RollingSuccessionPlanRequestWeight(8, true);
    }
}
