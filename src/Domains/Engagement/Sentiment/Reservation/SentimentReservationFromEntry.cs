namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Functions of sentiment reservation from entry.</summary>
public static class SentimentReservationFromEntry
{
    public static Dictionary<string, double> SummarizeGlobalActionPlanEntriesForReservation(List<GlobalActionPlanEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<SentimentReservation> DeriveSentimentReservationsFromEntry(List<GlobalActionPlanEntry> items)
    {
        var outValue = new List<SentimentReservation>();
        foreach (var item in items)
        {
            var derived = SentimentReservation.NewSentimentReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeSentimentReservation(SentimentReservation item)
    {
        return GlobalActionPlanEntryPolicy.ClassifyGlobalActionPlanEntry(item.WeightedLoad());
    }
}
