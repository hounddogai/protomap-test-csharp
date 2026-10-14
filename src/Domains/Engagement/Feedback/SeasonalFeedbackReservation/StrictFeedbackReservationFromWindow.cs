namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Functions of strict feedback reservation from window.</summary>
public static class StrictFeedbackReservationFromWindow
{
    public static Dictionary<string, double> SummarizeFeedbackWindowsForReservation(List<FeedbackWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<StrictFeedbackReservation> DeriveStrictFeedbackReservationsFromWindow(List<FeedbackWindow> items)
    {
        var outValue = new List<StrictFeedbackReservation>();
        foreach (var item in items)
        {
            var derived = StrictFeedbackReservation.NewStrictFeedbackReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateStrictFeedbackReservation(StrictFeedbackReservation item)
    {
        return FeedbackLedgerPolicy.NormalizeFeedbackLedgerCode(item.Code);
    }
}
