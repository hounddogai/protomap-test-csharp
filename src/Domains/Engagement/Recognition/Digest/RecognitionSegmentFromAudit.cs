namespace Acme.Workforce.Engagement.Recognition.Digest;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Functions of recognition segment from audit.</summary>
public static class RecognitionSegmentFromAudit
{
    public static Dictionary<string, double> SummarizeFeedbackAuditsForSegment(List<FeedbackAudit> items)
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

    public static List<RecognitionSegment> DeriveRecognitionSegmentsFromAudit(List<FeedbackAudit> items)
    {
        var outValue = new List<RecognitionSegment>();
        foreach (var item in items)
        {
            var derived = RecognitionSegment.NewRecognitionSegment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignRecognitionSegment(RecognitionSegment item)
    {
        return FeedbackClaimPolicy.FeedbackClaimBucket(item.ProjectedValue());
    }
}
