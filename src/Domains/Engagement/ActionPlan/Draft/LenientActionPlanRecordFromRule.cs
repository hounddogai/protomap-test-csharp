namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Functions of lenient action plan record from rule.</summary>
public static class LenientActionPlanRecordFromRule
{
    public static Dictionary<string, double> SummarizeDefaultFeedbackRulesForRecord(List<DefaultFeedbackRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<LenientActionPlanRecord> DeriveLenientActionPlanRecordsFromRule(List<DefaultFeedbackRule> items)
    {
        var outValue = new List<LenientActionPlanRecord>();
        foreach (var item in items)
        {
            var derived = LenientActionPlanRecord.NewLenientActionPlanRecord(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceLenientActionPlanRecord(LenientActionPlanRecord item)
    {
        return BatchedFeedbackBaselinePolicy.BatchedFeedbackBaselineWeight(6, true);
    }
}
