namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Functions of recognition ledger from reservation.</summary>
public static class RecognitionLedgerFromReservation
{
    public static Dictionary<string, double> SummarizeDefaultFeedbackReservationsForLedger(List<DefaultFeedbackReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<RecognitionLedger> DeriveRecognitionLedgersFromReservation(List<DefaultFeedbackReservation> items)
    {
        var outValue = new List<RecognitionLedger>();
        foreach (var item in items)
        {
            var derived = RecognitionLedger.NewRecognitionLedger(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeRecognitionLedger(RecognitionLedger item)
    {
        return SeasonalFeedbackClaimPolicy.SeasonalFeedbackClaimWeight(7, true);
    }
}
