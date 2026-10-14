namespace Acme.Workforce.Engagement.Feedback.Mapping;

using Acme.Workforce.Engagement.Recognition.Claim;

/// <summary>Functions of pending feedback milestone from claim.</summary>
public static class PendingFeedbackMilestoneFromClaim
{
    public static Dictionary<string, double> SummarizeRecognitionClaimsForMilestone(List<RecognitionClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<PendingFeedbackMilestone> DerivePendingFeedbackMilestonesFromClaim(List<RecognitionClaim> items)
    {
        var outValue = new List<PendingFeedbackMilestone>();
        foreach (var item in items)
        {
            var derived = PendingFeedbackMilestone.NewPendingFeedbackMilestone(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyPendingFeedbackMilestone(PendingFeedbackMilestone item)
    {
        return RecognitionClaimPolicy.RecognitionClaimBucket(item.AdjustedValue());
    }
}
