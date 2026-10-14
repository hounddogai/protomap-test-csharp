namespace Acme.Workforce.Engagement.ActionPlan.Group;

using Acme.Workforce.Engagement.Feedback.FeedbackGroup;

/// <summary>Functions of manual action plan entry from budget.</summary>
public static class ManualActionPlanEntryFromBudget
{
    public static Dictionary<string, double> SummarizeFeedbackBudgetsForEntry(List<FeedbackBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<ManualActionPlanEntry> DeriveManualActionPlanEntriesFromBudget(List<FeedbackBudget> items)
    {
        var outValue = new List<ManualActionPlanEntry>();
        foreach (var item in items)
        {
            var derived = ManualActionPlanEntry.NewManualActionPlanEntry(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleManualActionPlanEntry(ManualActionPlanEntry item)
    {
        return FeedbackBudgetPolicy.FeedbackBudgetBucket(item.NetLoad());
    }
}
