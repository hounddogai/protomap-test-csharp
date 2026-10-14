namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.Policy;

/// <summary>Functions of policy override from record.</summary>
public static class PolicyOverrideFromRecord
{
    public static Dictionary<string, double> SummarizeReturnPlanRecordsForOverride(List<ReturnPlanRecord> items)
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

    public static List<PolicyOverride> DerivePolicyOverridesFromRecord(List<ReturnPlanRecord> items)
    {
        var outValue = new List<PolicyOverride>();
        foreach (var item in items)
        {
            var derived = PolicyOverride.NewPolicyOverride(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScorePolicyOverride(PolicyOverride item)
    {
        return ReturnPlanQuotaPolicy.ReturnPlanQuotaBucket(item.EffectiveVolume());
    }
}
