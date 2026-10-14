namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Functions of recognition snapshot from reservation.</summary>
public static class RecognitionSnapshotFromReservation
{
    public static Dictionary<string, double> SummarizeStrictFeedbackReservationsForSnapshot(List<StrictFeedbackReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<RecognitionSnapshot> DeriveRecognitionSnapshotsFromReservation(List<StrictFeedbackReservation> items)
    {
        var outValue = new List<RecognitionSnapshot>();
        foreach (var item in items)
        {
            var derived = RecognitionSnapshot.NewRecognitionSnapshot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileRecognitionSnapshot(RecognitionSnapshot item)
    {
        return FeedbackDraftPolicy.NormalizeFeedbackDraftCode(item.Code);
    }
}
