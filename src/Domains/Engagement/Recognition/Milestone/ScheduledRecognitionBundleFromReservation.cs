namespace Acme.Workforce.Engagement.Recognition.Milestone;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>Functions of scheduled recognition bundle from reservation.</summary>
public static class ScheduledRecognitionBundleFromReservation
{
    public static Dictionary<string, double> SummarizeFallbackFeedbackReservationsForBundle(List<FallbackFeedbackReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<ScheduledRecognitionBundle> DeriveScheduledRecognitionBundlesFromReservation(List<FallbackFeedbackReservation> items)
    {
        var outValue = new List<ScheduledRecognitionBundle>();
        foreach (var item in items)
        {
            var derived = ScheduledRecognitionBundle.NewScheduledRecognitionBundle(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanScheduledRecognitionBundle(ScheduledRecognitionBundle item)
    {
        return FallbackFeedbackReservationPolicy.FallbackFeedbackReservationWeight(2, true);
    }
}
