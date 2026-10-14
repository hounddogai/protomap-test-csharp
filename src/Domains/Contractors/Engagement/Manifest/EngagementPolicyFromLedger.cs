namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>Functions of engagement policy from ledger.</summary>
public static class EngagementPolicyFromLedger
{
    public static Dictionary<string, double> SummarizeOffboardingLedgersForPolicy(List<OffboardingLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<EngagementPolicy> DeriveEngagementPoliciesFromLedger(List<OffboardingLedger> items)
    {
        var outValue = new List<EngagementPolicy>();
        foreach (var item in items)
        {
            var derived = EngagementPolicy.NewEngagementPolicy(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeEngagementPolicy(EngagementPolicy item)
    {
        return OffboardingLedgerPolicy.ClassifyOffboardingLedger(item.AdjustedLoad());
    }
}
