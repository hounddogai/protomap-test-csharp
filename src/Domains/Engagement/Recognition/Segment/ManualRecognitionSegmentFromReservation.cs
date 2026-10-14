namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Functions of manual recognition segment from reservation.</summary>
public static class ManualRecognitionSegmentFromReservation
{
    public static Dictionary<string, double> SummarizeFeedbackReservationsForSegment(List<FeedbackReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<ManualRecognitionSegment> DeriveManualRecognitionSegmentsFromReservation(List<FeedbackReservation> items)
    {
        var outValue = new List<ManualRecognitionSegment>();
        foreach (var item in items)
        {
            var derived = ManualRecognitionSegment.NewManualRecognitionSegment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitManualRecognitionSegment(ManualRecognitionSegment item)
    {
        return FeedbackAllocationPolicy.NormalizeFeedbackAllocationCode(item.Code);
    }
}
