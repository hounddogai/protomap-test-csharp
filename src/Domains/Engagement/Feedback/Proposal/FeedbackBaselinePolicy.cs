namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback baseline policy.</summary>
public static class FeedbackBaselinePolicy
{
    public static string FeedbackBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 22632.0, in order.</summary>
    public static int FeedbackBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22632.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a feedback baseline score falls in.</summary>
    public static string ClassifyFeedbackBaseline(double score)
    {
        if (score < 5.3)
        {
            return "watch";
        }
        if (score < 23.1)
        {
            return "high";
        }
        if (score < 32.1)
        {
            return "minimal";
        }
        return "critical";
    }
}
