namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Functions of return plan cycle from quota.</summary>
public static class ReturnPlanCycleFromQuota
{
    public static Dictionary<string, double> SummarizePolicyQuotasForCycle(List<PolicyQuota> items)
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

    public static List<ReturnPlanCycle> DeriveReturnPlanCyclesFromQuota(List<PolicyQuota> items)
    {
        var outValue = new List<ReturnPlanCycle>();
        foreach (var item in items)
        {
            var derived = ReturnPlanCycle.NewReturnPlanCycle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreReturnPlanCycle(ReturnPlanCycle item)
    {
        return PolicyQuotaPolicy.ClassifyPolicyQuota(item.NetValue());
    }
}
