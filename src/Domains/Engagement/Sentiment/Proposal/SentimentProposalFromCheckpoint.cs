namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Functions of sentiment proposal from checkpoint.</summary>
public static class SentimentProposalFromCheckpoint
{
    public static Dictionary<string, double> SummarizeFeedbackCheckpointsForProposal(List<FeedbackCheckpoint> items)
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

    public static List<SentimentProposal> DeriveSentimentProposalsFromCheckpoint(List<FeedbackCheckpoint> items)
    {
        var outValue = new List<SentimentProposal>();
        foreach (var item in items)
        {
            var derived = SentimentProposal.NewSentimentProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitSentimentProposal(SentimentProposal item)
    {
        return SeasonalFeedbackMilestonePolicy.NormalizeSeasonalFeedbackMilestoneCode(item.Code);
    }
}
