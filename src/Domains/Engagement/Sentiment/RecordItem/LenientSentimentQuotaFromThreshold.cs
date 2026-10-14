namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>Functions of lenient sentiment quota from threshold.</summary>
public static class LenientSentimentQuotaFromThreshold
{
    public static Dictionary<string, double> SummarizeLenientSentimentThresholdsForQuota(List<LenientSentimentThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<LenientSentimentQuota> DeriveLenientSentimentQuotasFromThreshold(List<LenientSentimentThreshold> items)
    {
        var outValue = new List<LenientSentimentQuota>();
        foreach (var item in items)
        {
            var derived = LenientSentimentQuota.NewLenientSentimentQuota(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitLenientSentimentQuota(LenientSentimentQuota item)
    {
        return LenientSentimentThresholdPolicy.LenientSentimentThresholdWeight(3, true);
    }
}
