namespace Acme.Workforce.Compliance.Audit.Policy;

using Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>Functions of seasonal audit preference from claim.</summary>
public static class SeasonalAuditPreferenceFromClaim
{
    public static Dictionary<string, double> SummarizeStrictPolicyAckClaimsForPreference(List<StrictPolicyAckClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<SeasonalAuditPreference> DeriveSeasonalAuditPreferencesFromClaim(List<StrictPolicyAckClaim> items)
    {
        var outValue = new List<SeasonalAuditPreference>();
        foreach (var item in items)
        {
            var derived = SeasonalAuditPreference.NewSeasonalAuditPreference(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectSeasonalAuditPreference(SeasonalAuditPreference item)
    {
        return LenientPolicyAckMilestonePolicy.LenientPolicyAckMilestoneBucket(item.EffectiveValue());
    }
}
