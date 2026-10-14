namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Functions of feedback ledger from reservation.</summary>
public static class FeedbackLedgerFromReservation
{
    public static Dictionary<string, double> SummarizeRollingRecognitionReservationsForLedger(List<RollingRecognitionReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<FeedbackLedger> DeriveFeedbackLedgersFromReservation(List<RollingRecognitionReservation> items)
    {
        var outValue = new List<FeedbackLedger>();
        foreach (var item in items)
        {
            var derived = FeedbackLedger.NewFeedbackLedger(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeFeedbackLedger(FeedbackLedger item)
    {
        return WeightedRecognitionVariancePolicy.WeightedRecognitionVarianceBucket(item.AdjustedTotal());
    }
}
