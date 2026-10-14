namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Functions of seasonal succession plan budget from slot.</summary>
public static class SeasonalSuccessionPlanBudgetFromSlot
{
    public static Dictionary<string, double> SummarizePendingPoolSlotsForBudget(List<PendingPoolSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<SeasonalSuccessionPlanBudget> DeriveSeasonalSuccessionPlanBudgetsFromSlot(List<PendingPoolSlot> items)
    {
        var outValue = new List<SeasonalSuccessionPlanBudget>();
        foreach (var item in items)
        {
            var derived = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceSeasonalSuccessionPlanBudget(SeasonalSuccessionPlanBudget item)
    {
        return FallbackPoolMetricPolicy.FallbackPoolMetricWeight(7, true);
    }
}
