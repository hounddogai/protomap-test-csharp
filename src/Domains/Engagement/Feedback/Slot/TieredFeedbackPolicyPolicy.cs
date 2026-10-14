namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered feedback policy policy.</summary>
public static class TieredFeedbackPolicyPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 97430.0, in order.</summary>
    public static int TieredFeedbackPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97430.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredFeedbackPolicy()
    {
        var policy = Retry.DefaultPolicy("tiered-feedback-policy");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int TieredFeedbackPolicySpan(int start, int end)
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

    /// <summary>Names the band a tiered feedback policy score falls in.</summary>
    public static string ClassifyTieredFeedbackPolicy(double score)
    {
        if (score < 13.5)
        {
            return "watch";
        }
        if (score < 61.2)
        {
            return "critical";
        }
        return "medium";
    }
}
