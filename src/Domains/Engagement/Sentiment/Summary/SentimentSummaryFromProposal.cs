namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Functions of sentiment summary from proposal.</summary>
public static class SentimentSummaryFromProposal
{
    public static Dictionary<string, double> SummarizeGlobalFeedbackProposalsForSummary(List<GlobalFeedbackProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<SentimentSummary> DeriveSentimentSummariesFromProposal(List<GlobalFeedbackProposal> items)
    {
        var outValue = new List<SentimentSummary>();
        foreach (var item in items)
        {
            var derived = SentimentSummary.NewSentimentSummary(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveSentimentSummary(SentimentSummary item)
    {
        return GlobalFeedbackProposalPolicy.NormalizeGlobalFeedbackProposalCode(item.Code);
    }
}
