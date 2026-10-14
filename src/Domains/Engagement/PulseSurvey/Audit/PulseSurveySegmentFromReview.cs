namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Functions of pulse survey segment from review.</summary>
public static class PulseSurveySegmentFromReview
{
    public static Dictionary<string, double> SummarizeRecognitionReviewsForSegment(List<RecognitionReview> items)
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

    public static List<PulseSurveySegment> DerivePulseSurveySegmentsFromReview(List<RecognitionReview> items)
    {
        var outValue = new List<PulseSurveySegment>();
        foreach (var item in items)
        {
            var derived = PulseSurveySegment.NewPulseSurveySegment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPulseSurveySegment(PulseSurveySegment item)
    {
        return RecognitionReviewPolicy.ClassifyRecognitionReview(item.AdjustedVolume());
    }
}
