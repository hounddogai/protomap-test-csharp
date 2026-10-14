namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Compensation.BonusPool.Rule;

/// <summary>Functions of scheduled equity grant plan from score.</summary>
public static class ScheduledEquityGrantPlanFromScore
{
    public static Dictionary<string, double> SummarizeWeightedBonusPoolScoresForPlan(List<WeightedBonusPoolScore> items)
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

    public static List<ScheduledEquityGrantPlan> DeriveScheduledEquityGrantPlansFromScore(List<WeightedBonusPoolScore> items)
    {
        var outValue = new List<ScheduledEquityGrantPlan>();
        foreach (var item in items)
        {
            var derived = ScheduledEquityGrantPlan.NewScheduledEquityGrantPlan(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyScheduledEquityGrantPlan(ScheduledEquityGrantPlan item)
    {
        return TieredBonusPoolQuotaPolicy.NormalizeTieredBonusPoolQuotaCode(item.Code);
    }
}
