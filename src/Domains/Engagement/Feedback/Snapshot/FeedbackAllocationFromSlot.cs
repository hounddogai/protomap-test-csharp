namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Functions of feedback allocation from slot.</summary>
public static class FeedbackAllocationFromSlot
{
    public static Dictionary<string, double> SummarizeManualActionPlanSlotsForAllocation(List<ManualActionPlanSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<FeedbackAllocation> DeriveFeedbackAllocationsFromSlot(List<ManualActionPlanSlot> items)
    {
        var outValue = new List<FeedbackAllocation>();
        foreach (var item in items)
        {
            var derived = FeedbackAllocation.NewFeedbackAllocation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeFeedbackAllocation(FeedbackAllocation item)
    {
        return PrimaryActionPlanSlotPolicy.PrimaryActionPlanSlotBucket(item.MeasuredValue());
    }
}
