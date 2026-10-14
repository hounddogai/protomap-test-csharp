namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Functions of policy draft from override.</summary>
public static class PolicyDraftFromOverride
{
    public static Dictionary<string, double> SummarizeLenientReturnPlanOverridesForDraft(List<LenientReturnPlanOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<PolicyDraft> DerivePolicyDraftsFromOverride(List<LenientReturnPlanOverride> items)
    {
        var outValue = new List<PolicyDraft>();
        foreach (var item in items)
        {
            var derived = PolicyDraft.NewPolicyDraft(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePolicyDraft(PolicyDraft item)
    {
        return LenientReturnPlanOverridePolicy.NormalizeLenientReturnPlanOverrideCode(item.Code);
    }
}
