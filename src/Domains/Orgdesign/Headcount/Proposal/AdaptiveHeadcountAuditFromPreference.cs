namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Functions of adaptive headcount audit from preference.</summary>
public static class AdaptiveHeadcountAuditFromPreference
{
    public static Dictionary<string, double> SummarizeRollingCostModelPreferencesForAudit(List<RollingCostModelPreference> items)
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

    public static List<AdaptiveHeadcountAudit> DeriveAdaptiveHeadcountAuditsFromPreference(List<RollingCostModelPreference> items)
    {
        var outValue = new List<AdaptiveHeadcountAudit>();
        foreach (var item in items)
        {
            var derived = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateAdaptiveHeadcountAudit(AdaptiveHeadcountAudit item)
    {
        return RollingCostModelPreferencePolicy.ClassifyRollingCostModelPreference(item.NetLoad());
    }
}
