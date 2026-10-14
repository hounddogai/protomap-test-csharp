namespace Acme.Workforce.Engagement.Feedback.Baseline;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Functions of feedback summary from manifest.</summary>
public static class FeedbackSummaryFromManifest
{
    public static Dictionary<string, double> SummarizeWeightedTownHallManifestsForSummary(List<WeightedTownHallManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<FeedbackSummary> DeriveFeedbackSummariesFromManifest(List<WeightedTownHallManifest> items)
    {
        var outValue = new List<FeedbackSummary>();
        foreach (var item in items)
        {
            var derived = FeedbackSummary.NewFeedbackSummary(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyFeedbackSummary(FeedbackSummary item)
    {
        return WeightedTownHallManifestPolicy.WeightedTownHallManifestWeight(4, true);
    }
}
