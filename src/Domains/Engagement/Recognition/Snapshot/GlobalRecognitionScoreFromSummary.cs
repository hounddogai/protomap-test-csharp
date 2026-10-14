namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Functions of global recognition score from summary.</summary>
public static class GlobalRecognitionScoreFromSummary
{
    public static Dictionary<string, double> SummarizeCappedFeedbackSummariesForScore(List<CappedFeedbackSummary> items)
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

    public static List<GlobalRecognitionScore> DeriveGlobalRecognitionScoresFromSummary(List<CappedFeedbackSummary> items)
    {
        var outValue = new List<GlobalRecognitionScore>();
        foreach (var item in items)
        {
            var derived = GlobalRecognitionScore.NewGlobalRecognitionScore(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreGlobalRecognitionScore(GlobalRecognitionScore item)
    {
        return FeedbackTargetPolicy.FeedbackTargetWeight(1, true);
    }
}
