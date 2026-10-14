namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Functions of scheduled pool window from claim.</summary>
public static class ScheduledPoolWindowFromClaim
{
    public static Dictionary<string, double> SummarizePrimarySuccessionPlanClaimsForWindow(List<PrimarySuccessionPlanClaim> items)
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

    public static List<ScheduledPoolWindow> DeriveScheduledPoolWindowsFromClaim(List<PrimarySuccessionPlanClaim> items)
    {
        var outValue = new List<ScheduledPoolWindow>();
        foreach (var item in items)
        {
            var derived = ScheduledPoolWindow.NewScheduledPoolWindow(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateScheduledPoolWindow(ScheduledPoolWindow item)
    {
        return CappedSuccessionPlanCyclePolicy.CappedSuccessionPlanCycleWeight(7, true);
    }
}
