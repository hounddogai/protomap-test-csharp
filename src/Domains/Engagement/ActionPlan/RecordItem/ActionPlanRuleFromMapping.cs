namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>Functions of action plan rule from mapping.</summary>
public static class ActionPlanRuleFromMapping
{
    public static Dictionary<string, double> SummarizeFeedbackMappingsForRule(List<FeedbackMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<ActionPlanRule> DeriveActionPlanRulesFromMapping(List<FeedbackMapping> items)
    {
        var outValue = new List<ActionPlanRule>();
        foreach (var item in items)
        {
            var derived = ActionPlanRule.NewActionPlanRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeActionPlanRule(ActionPlanRule item)
    {
        return PendingFeedbackMilestonePolicy.PendingFeedbackMilestoneWeight(7, true);
    }
}
