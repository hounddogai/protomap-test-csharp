namespace Acme.Workforce.Engagement.ActionPlan.Policy;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Functions of default action plan policy from entry.</summary>
public static class DefaultActionPlanPolicyFromEntry
{
    public static Dictionary<string, double> SummarizeManualActionPlanEntriesForPolicy(List<ManualActionPlanEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<DefaultActionPlanPolicy> DeriveDefaultActionPlanPoliciesFromEntry(List<ManualActionPlanEntry> items)
    {
        var outValue = new List<DefaultActionPlanPolicy>();
        foreach (var item in items)
        {
            var derived = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitDefaultActionPlanPolicy(DefaultActionPlanPolicy item)
    {
        return ManualActionPlanEntryPolicy.ClassifyManualActionPlanEntry(item.AdjustedTotal());
    }
}
