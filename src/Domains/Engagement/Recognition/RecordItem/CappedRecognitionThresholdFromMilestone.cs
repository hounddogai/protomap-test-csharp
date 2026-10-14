namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Functions of capped recognition threshold from milestone.</summary>
public static class CappedRecognitionThresholdFromMilestone
{
    public static Dictionary<string, double> SummarizeAdaptiveActionPlanMilestonesForThreshold(List<AdaptiveActionPlanMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<CappedRecognitionThreshold> DeriveCappedRecognitionThresholdsFromMilestone(List<AdaptiveActionPlanMilestone> items)
    {
        var outValue = new List<CappedRecognitionThreshold>();
        foreach (var item in items)
        {
            var derived = CappedRecognitionThreshold.NewCappedRecognitionThreshold(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCappedRecognitionThreshold(CappedRecognitionThreshold item)
    {
        return ScheduledActionPlanSlotPolicy.ScheduledActionPlanSlotBucket(item.WeightedLoad());
    }
}
