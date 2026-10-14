namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;

/// <summary>Functions of tiered return plan rule from digest.</summary>
public static class TieredReturnPlanRuleFromDigest
{
    public static Dictionary<string, double> SummarizePolicyDigestsForRule(List<PolicyDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<TieredReturnPlanRule> DeriveTieredReturnPlanRulesFromDigest(List<PolicyDigest> items)
    {
        var outValue = new List<TieredReturnPlanRule>();
        foreach (var item in items)
        {
            var derived = TieredReturnPlanRule.NewTieredReturnPlanRule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeTieredReturnPlanRule(TieredReturnPlanRule item)
    {
        return PolicyDigestPolicy.NormalizePolicyDigestCode(item.Code);
    }
}
