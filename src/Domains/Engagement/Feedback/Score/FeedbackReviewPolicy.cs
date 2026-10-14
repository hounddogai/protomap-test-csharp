namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback review policy.</summary>
public static class FeedbackReviewPolicy
{
    public static string FeedbackReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a feedback review code in its canonical FR- form.</summary>
    public static string NormalizeFeedbackReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 16717.0, in order.</summary>
    public static int FeedbackReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16717.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
