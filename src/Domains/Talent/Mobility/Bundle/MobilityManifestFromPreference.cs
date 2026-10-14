namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Talent.SuccessionPlan.Budget;

/// <summary>Functions of mobility manifest from preference.</summary>
public static class MobilityManifestFromPreference
{
    public static Dictionary<string, double> SummarizeSuccessionPlanPreferencesForManifest(List<SuccessionPlanPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<MobilityManifest> DeriveMobilityManifestsFromPreference(List<SuccessionPlanPreference> items)
    {
        var outValue = new List<MobilityManifest>();
        foreach (var item in items)
        {
            var derived = MobilityManifest.NewMobilityManifest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchMobilityManifest(MobilityManifest item)
    {
        return SuccessionPlanPolicyPolicy.SuccessionPlanPolicyBucket(item.MeasuredValue());
    }
}
