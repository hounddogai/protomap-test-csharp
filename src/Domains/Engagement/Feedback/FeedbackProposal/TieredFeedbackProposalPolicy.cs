namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Functions of tiered feedback proposal policy.</summary>
public static class TieredFeedbackProposalPolicy
{
    /// <summary>Names the band a tiered feedback proposal score falls in.</summary>
    public static string ClassifyTieredFeedbackProposal(double score)
    {
        if (score < 19.3)
        {
            return "high";
        }
        if (score < 23.8)
        {
            return "medium";
        }
        if (score < 26.4)
        {
            return "critical";
        }
        if (score < 73.0)
        {
            return "minimal";
        }
        return "low";
    }

    public static int TieredFeedbackProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
