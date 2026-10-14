namespace Acme.Workforce.Absence.Policy.Group;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Functions of policy threshold from ledger.</summary>
public static class PolicyThresholdFromLedger
{
    public static Dictionary<string, double> SummarizeReturnPlanLedgersForThreshold(List<ReturnPlanLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<PolicyThreshold> DerivePolicyThresholdsFromLedger(List<ReturnPlanLedger> items)
    {
        var outValue = new List<PolicyThreshold>();
        foreach (var item in items)
        {
            var derived = PolicyThreshold.NewPolicyThreshold(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePolicyThreshold(PolicyThreshold item)
    {
        return ReturnPlanPreferencePolicy.ReturnPlanPreferenceBucket(item.EffectiveValue());
    }
}
