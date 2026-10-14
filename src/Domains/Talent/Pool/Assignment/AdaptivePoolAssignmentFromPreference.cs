namespace Acme.Workforce.Talent.Pool.Assignment;

using Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Functions of adaptive pool assignment from preference.</summary>
public static class AdaptivePoolAssignmentFromPreference
{
    public static Dictionary<string, double> SummarizeSeasonalSuccessionPlanPreferencesForAssignment(List<SeasonalSuccessionPlanPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<AdaptivePoolAssignment> DeriveAdaptivePoolAssignmentsFromPreference(List<SeasonalSuccessionPlanPreference> items)
    {
        var outValue = new List<AdaptivePoolAssignment>();
        foreach (var item in items)
        {
            var derived = AdaptivePoolAssignment.NewAdaptivePoolAssignment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignAdaptivePoolAssignment(AdaptivePoolAssignment item)
    {
        return PrimarySuccessionPlanRecordPolicy.NormalizePrimarySuccessionPlanRecordCode(item.Code);
    }
}
