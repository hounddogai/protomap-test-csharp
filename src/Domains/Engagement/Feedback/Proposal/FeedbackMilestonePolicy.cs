namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback milestone policy.</summary>
public static class FeedbackMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 49433.0, in order.</summary>
    public static int FeedbackMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49433.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a feedback milestone code in its canonical FM- form.</summary>
    public static string NormalizeFeedbackMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double FeedbackMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
