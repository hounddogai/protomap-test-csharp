namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Functions of engagement budget from preference.</summary>
public static class EngagementBudgetFromPreference
{
    public static Dictionary<string, double> SummarizeEngagementPreferencesForBudget(List<EngagementPreference> items)
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

    public static List<EngagementBudget> DeriveEngagementBudgetsFromPreference(List<EngagementPreference> items)
    {
        var outValue = new List<EngagementBudget>();
        foreach (var item in items)
        {
            var derived = EngagementBudget.NewEngagementBudget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyEngagementBudgetExtra(EngagementBudget item)
    {
        return EngagementAssignmentPolicy.EngagementAssignmentBucket(item.EffectiveTotal());
    }
}
