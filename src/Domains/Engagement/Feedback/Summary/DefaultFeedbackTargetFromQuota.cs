namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Engagement.Sentiment.RecordItem;

/// <summary>Functions of default feedback target from quota.</summary>
public static class DefaultFeedbackTargetFromQuota
{
    public static Dictionary<string, double> SummarizeLenientSentimentQuotasForTarget(List<LenientSentimentQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<DefaultFeedbackTarget> DeriveDefaultFeedbackTargetsFromQuota(List<LenientSentimentQuota> items)
    {
        var outValue = new List<DefaultFeedbackTarget>();
        foreach (var item in items)
        {
            var derived = DefaultFeedbackTarget.NewDefaultFeedbackTarget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeDefaultFeedbackTarget(DefaultFeedbackTarget item)
    {
        return SentimentRecordPolicy.SentimentRecordBucket(item.AdjustedValue());
    }
}
