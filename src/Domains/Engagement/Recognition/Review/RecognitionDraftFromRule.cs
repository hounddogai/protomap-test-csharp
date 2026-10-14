namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Functions of recognition draft from rule.</summary>
public static class RecognitionDraftFromRule
{
    public static Dictionary<string, double> SummarizeActionPlanRulesForDraft(List<ActionPlanRule> items)
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

    public static List<RecognitionDraft> DeriveRecognitionDraftsFromRule(List<ActionPlanRule> items)
    {
        var outValue = new List<RecognitionDraft>();
        foreach (var item in items)
        {
            var derived = RecognitionDraft.NewRecognitionDraft(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileRecognitionDraft(RecognitionDraft item)
    {
        return ActionPlanRulePolicy.ActionPlanRuleWeight(3, true);
    }
}
