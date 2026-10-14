namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive feedback score policy.</summary>
public static class AdaptiveFeedbackScorePolicy
{
    public static int AdaptiveFeedbackScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveFeedbackScore()
    {
        var policy = Retry.DefaultPolicy("adaptive-feedback-score");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 4073.0, in order.</summary>
    public static int AdaptiveFeedbackScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4073.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
