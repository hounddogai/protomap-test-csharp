namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.RecordItem;

/// <summary>Functions of pending sentiment variance from record.</summary>
public static class PendingSentimentVarianceFromRecord
{
    public static Dictionary<string, double> SummarizeSentimentRecordsForVariance(List<SentimentRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<PendingSentimentVariance> DerivePendingSentimentVariancesFromRecord(List<SentimentRecord> items)
    {
        var outValue = new List<PendingSentimentVariance>();
        foreach (var item in items)
        {
            var derived = PendingSentimentVariance.NewPendingSentimentVariance(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyPendingSentimentVariance(PendingSentimentVariance item)
    {
        return SentimentRecordPolicy.SentimentRecordBucket(item.AdjustedValue());
    }
}
