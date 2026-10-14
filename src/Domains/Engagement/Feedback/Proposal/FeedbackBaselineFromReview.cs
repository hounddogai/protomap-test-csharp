namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Functions of feedback baseline from review.</summary>
public static class FeedbackBaselineFromReview
{
    public static Dictionary<string, double> SummarizeRecognitionReviewsForBaseline(List<RecognitionReview> items)
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

    public static List<FeedbackBaseline> DeriveFeedbackBaselinesFromReview(List<RecognitionReview> items)
    {
        var outValue = new List<FeedbackBaseline>();
        foreach (var item in items)
        {
            var derived = FeedbackBaseline.NewFeedbackBaseline(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchFeedbackBaseline(FeedbackBaseline item)
    {
        return RecognitionBundlePolicy.RecognitionBundleWeight(5, true);
    }
}
