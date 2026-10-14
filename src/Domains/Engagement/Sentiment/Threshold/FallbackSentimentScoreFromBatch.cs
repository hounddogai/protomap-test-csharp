namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using Acme.Workforce.Engagement.Feedback.Group;
using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Functions of fallback sentiment score from batch.</summary>
public static class FallbackSentimentScoreFromBatch
{
    public static Dictionary<string, double> SummarizeLenientRecognitionBatchesForScore(List<LenientRecognitionBatch> items)
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

    public static List<FallbackSentimentScore> DeriveFallbackSentimentScoresFromBatch(List<LenientRecognitionBatch> items)
    {
        var outValue = new List<FallbackSentimentScore>();
        foreach (var item in items)
        {
            var derived = FallbackSentimentScore.NewFallbackSentimentScore(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignFallbackSentimentScore(FallbackSentimentScore item)
    {
        return AdaptiveRecognitionBatchPolicy.AdaptiveRecognitionBatchWeight(2, true);
    }

    public static Dictionary<string, double> SummarizeBatchedFeedbackBatchesForScore(List<BatchedFeedbackBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<FallbackSentimentScore> DeriveFallbackSentimentScoresFromBatchExtra(List<BatchedFeedbackBatch> items)
    {
        var outValue = new List<FallbackSentimentScore>();
        foreach (var item in items)
        {
            var derived = FallbackSentimentScore.NewFallbackSentimentScore(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchFallbackSentimentScore(FallbackSentimentScore item)
    {
        return FeedbackGroupPolicy.FeedbackGroupWeight(9, true);
    }
}
