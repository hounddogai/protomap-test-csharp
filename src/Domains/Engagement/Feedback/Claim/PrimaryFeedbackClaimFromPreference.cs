namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Functions of primary feedback claim from preference.</summary>
public static class PrimaryFeedbackClaimFromPreference
{
    public static Dictionary<string, double> SummarizeActionPlanPreferencesForClaim(List<ActionPlanPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<PrimaryFeedbackClaim> DerivePrimaryFeedbackClaimsFromPreference(List<ActionPlanPreference> items)
    {
        var outValue = new List<PrimaryFeedbackClaim>();
        foreach (var item in items)
        {
            var derived = PrimaryFeedbackClaim.NewPrimaryFeedbackClaim(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePrimaryFeedbackClaim(PrimaryFeedbackClaim item)
    {
        return ActionPlanBaselinePolicy.ActionPlanBaselineBucket(item.AdjustedValue());
    }
}
