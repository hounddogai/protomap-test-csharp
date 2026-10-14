namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>Functions of fallback action plan draft from record.</summary>
public static class FallbackActionPlanDraftFromRecord
{
    public static Dictionary<string, double> SummarizeGlobalPulseSurveyRecordsForDraft(List<GlobalPulseSurveyRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<FallbackActionPlanDraft> DeriveFallbackActionPlanDraftsFromRecord(List<GlobalPulseSurveyRecord> items)
    {
        var outValue = new List<FallbackActionPlanDraft>();
        foreach (var item in items)
        {
            var derived = FallbackActionPlanDraft.NewFallbackActionPlanDraft(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyFallbackActionPlanDraft(FallbackActionPlanDraft item)
    {
        return GlobalPulseSurveyRecordPolicy.GlobalPulseSurveyRecordWeight(4, true);
    }
}
