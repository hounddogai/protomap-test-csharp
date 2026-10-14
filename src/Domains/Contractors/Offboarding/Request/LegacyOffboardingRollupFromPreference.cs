namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Functions of legacy offboarding rollup from preference.</summary>
public static class LegacyOffboardingRollupFromPreference
{
    public static Dictionary<string, double> SummarizeEngagementPreferencesForRollup(List<EngagementPreference> items)
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

    public static List<LegacyOffboardingRollup> DeriveLegacyOffboardingRollupsFromPreference(List<EngagementPreference> items)
    {
        var outValue = new List<LegacyOffboardingRollup>();
        foreach (var item in items)
        {
            var derived = LegacyOffboardingRollup.NewLegacyOffboardingRollup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeLegacyOffboardingRollup(LegacyOffboardingRollup item)
    {
        return EngagementAssignmentPolicy.ClassifyEngagementAssignment(item.MeasuredValue());
    }
}
