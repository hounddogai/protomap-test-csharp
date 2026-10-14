namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Functions of lenient return plan override from override.</summary>
public static class LenientReturnPlanOverrideFromOverride
{
    public static Dictionary<string, double> SummarizeScheduledAccrualOverridesForOverride(List<ScheduledAccrualOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<LenientReturnPlanOverride> DeriveLenientReturnPlanOverridesFromOverride(List<ScheduledAccrualOverride> items)
    {
        var outValue = new List<LenientReturnPlanOverride>();
        foreach (var item in items)
        {
            var derived = LenientReturnPlanOverride.NewLenientReturnPlanOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceLenientReturnPlanOverride(LenientReturnPlanOverride item)
    {
        return ScheduledAccrualOverridePolicy.ClassifyScheduledAccrualOverride(item.EffectiveTotal());
    }
}
