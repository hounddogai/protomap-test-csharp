namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy feedback claim policy.</summary>
public static class LegacyFeedbackClaimPolicy
{
    public static int LegacyFeedbackClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    public static double LegacyFeedbackClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 10895.0, in order.</summary>
    public static int LegacyFeedbackClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10895.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a legacy feedback claim score falls in.</summary>
    public static string ClassifyLegacyFeedbackClaim(double score)
    {
        if (score < 44.6)
        {
            return "high";
        }
        if (score < 63.1)
        {
            return "ok";
        }
        if (score < 88.0)
        {
            return "watch";
        }
        return "minimal";
    }
}
