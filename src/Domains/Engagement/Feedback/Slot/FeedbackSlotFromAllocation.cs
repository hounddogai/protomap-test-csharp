namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Engagement.Recognition.Agreement;

/// <summary>Functions of feedback slot from allocation.</summary>
public static class FeedbackSlotFromAllocation
{
    public static Dictionary<string, double> SummarizePendingRecognitionAllocationsForSlot(List<PendingRecognitionAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<FeedbackSlot> DeriveFeedbackSlotsFromAllocation(List<PendingRecognitionAllocation> items)
    {
        var outValue = new List<FeedbackSlot>();
        foreach (var item in items)
        {
            var derived = FeedbackSlot.NewFeedbackSlot(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankFeedbackSlot(FeedbackSlot item)
    {
        return RecognitionAgreementPolicy.NormalizeRecognitionAgreementCode(item.Code);
    }
}
