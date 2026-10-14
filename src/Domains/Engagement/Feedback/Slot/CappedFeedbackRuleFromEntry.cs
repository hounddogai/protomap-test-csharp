namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Functions of capped feedback rule from entry.</summary>
public static class CappedFeedbackRuleFromEntry
{
    public static Dictionary<string, double> SummarizeManualActionPlanEntriesForRule(List<ManualActionPlanEntry> items)
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

    public static List<CappedFeedbackRule> DeriveCappedFeedbackRulesFromEntry(List<ManualActionPlanEntry> items)
    {
        var outValue = new List<CappedFeedbackRule>();
        foreach (var item in items)
        {
            var derived = CappedFeedbackRule.NewCappedFeedbackRule(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeCappedFeedbackRule(CappedFeedbackRule item)
    {
        return ActionPlanGroupPolicy.ActionPlanGroupBucket(item.ProjectedValue());
    }
}
