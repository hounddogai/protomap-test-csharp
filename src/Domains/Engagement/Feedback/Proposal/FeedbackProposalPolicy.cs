namespace Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Functions of feedback proposal policy.</summary>
public static class FeedbackProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 29817.0, in order.</summary>
    public static int FeedbackProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29817.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a feedback proposal score falls in.</summary>
    public static string ClassifyFeedbackProposal(double score)
    {
        if (score < 19.4)
        {
            return "minimal";
        }
        if (score < 73.8)
        {
            return "watch";
        }
        return "critical";
    }
}
