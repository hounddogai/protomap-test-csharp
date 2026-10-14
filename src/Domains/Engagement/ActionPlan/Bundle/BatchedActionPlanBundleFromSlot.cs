namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Functions of batched action plan bundle from slot.</summary>
public static class BatchedActionPlanBundleFromSlot
{
    public static Dictionary<string, double> SummarizeAdaptiveActionPlanSlotsForBundle(List<AdaptiveActionPlanSlot> items)
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

    public static List<BatchedActionPlanBundle> DeriveBatchedActionPlanBundlesFromSlot(List<AdaptiveActionPlanSlot> items)
    {
        var outValue = new List<BatchedActionPlanBundle>();
        foreach (var item in items)
        {
            var derived = BatchedActionPlanBundle.NewBatchedActionPlanBundle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitBatchedActionPlanBundle(BatchedActionPlanBundle item)
    {
        return AdaptiveActionPlanSlotPolicy.AdaptiveActionPlanSlotWeight(8, true);
    }
}
