namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Functions of pulse survey review from tier.</summary>
public static class PulseSurveyReviewFromTier
{
    public static Dictionary<string, double> SummarizeFeedbackTiersForReview(List<FeedbackTier> items)
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

    public static List<PulseSurveyReview> DerivePulseSurveyReviewsFromTier(List<FeedbackTier> items)
    {
        var outValue = new List<PulseSurveyReview>();
        foreach (var item in items)
        {
            var derived = PulseSurveyReview.NewPulseSurveyReview(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettlePulseSurveyReview(PulseSurveyReview item)
    {
        return FeedbackThresholdPolicy.FeedbackThresholdWeight(7, true);
    }
}
