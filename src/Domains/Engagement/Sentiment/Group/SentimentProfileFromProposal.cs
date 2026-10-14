namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Functions of sentiment profile from proposal.</summary>
public static class SentimentProfileFromProposal
{
    public static Dictionary<string, double> SummarizeTieredFeedbackProposalsForProfile(List<TieredFeedbackProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<SentimentProfile> DeriveSentimentProfilesFromProposal(List<TieredFeedbackProposal> items)
    {
        var outValue = new List<SentimentProfile>();
        foreach (var item in items)
        {
            var derived = SentimentProfile.NewSentimentProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectSentimentProfile(SentimentProfile item)
    {
        return TieredFeedbackProposalPolicy.ClassifyTieredFeedbackProposal(item.MeasuredValue());
    }
}
