namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Functions of sentiment baseline from slot.</summary>
public static class SentimentBaselineFromSlot
{
    public static Dictionary<string, double> SummarizeAdaptiveActionPlanSlotsForBaseline(List<AdaptiveActionPlanSlot> items)
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

    public static List<SentimentBaseline> DeriveSentimentBaselinesFromSlot(List<AdaptiveActionPlanSlot> items)
    {
        var outValue = new List<SentimentBaseline>();
        foreach (var item in items)
        {
            var derived = SentimentBaseline.NewSentimentBaseline(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleSentimentBaseline(SentimentBaseline item)
    {
        return DefaultActionPlanManifestPolicy.ClassifyDefaultActionPlanManifest(item.EffectiveExposure());
    }
}
