namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Functions of assignment preference from metric.</summary>
public static class AssignmentPreferenceFromMetric
{
    public static Dictionary<string, double> SummarizeDefaultRefreshMetricsForPreference(List<DefaultRefreshMetric> items)
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

    public static List<AssignmentPreference> DeriveAssignmentPreferencesFromMetric(List<DefaultRefreshMetric> items)
    {
        var outValue = new List<AssignmentPreference>();
        foreach (var item in items)
        {
            var derived = AssignmentPreference.NewAssignmentPreference(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAssignmentPreference(AssignmentPreference item)
    {
        return WeightedRefreshScorePolicy.NormalizeWeightedRefreshScoreCode(item.Code);
    }
}
