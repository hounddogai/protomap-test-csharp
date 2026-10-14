namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using Acme.Workforce.Engagement.Feedback.Baseline;

/// <summary>Functions of sentiment reservation from baseline.</summary>
public static class SentimentReservationFromBaseline
{
    public static Dictionary<string, double> SummarizeScheduledFeedbackBaselinesForReservation(List<ScheduledFeedbackBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<SentimentReservation> DeriveSentimentReservationsFromBaseline(List<ScheduledFeedbackBaseline> items)
    {
        var outValue = new List<SentimentReservation>();
        foreach (var item in items)
        {
            var derived = SentimentReservation.NewSentimentReservation(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectSentimentReservation(SentimentReservation item)
    {
        return ScheduledFeedbackBaselinePolicy.ClassifyScheduledFeedbackBaseline(item.WeightedLoad());
    }
}
