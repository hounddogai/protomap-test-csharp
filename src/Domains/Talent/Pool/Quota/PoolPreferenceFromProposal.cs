namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Functions of pool preference from proposal.</summary>
public static class PoolPreferenceFromProposal
{
    public static Dictionary<string, double> SummarizeSuccessionPlanProposalsForPreference(List<SuccessionPlanProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<PoolPreference> DerivePoolPreferencesFromProposal(List<SuccessionPlanProposal> items)
    {
        var outValue = new List<PoolPreference>();
        foreach (var item in items)
        {
            var derived = PoolPreference.NewPoolPreference(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePoolPreference(PoolPreference item)
    {
        return RegionalSuccessionPlanSummaryPolicy.ClassifyRegionalSuccessionPlanSummary(item.WeightedLoad());
    }
}
