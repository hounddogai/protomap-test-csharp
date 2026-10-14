namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default feedback target policy.</summary>
public static class DefaultFeedbackTargetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 18634.0, in order.</summary>
    public static int DefaultFeedbackTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 18634.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a default feedback target score falls in.</summary>
    public static string ClassifyDefaultFeedbackTarget(double score)
    {
        if (score < 10.6)
        {
            return "critical";
        }
        if (score < 57.5)
        {
            return "minimal";
        }
        if (score < 62.5)
        {
            return "high";
        }
        if (score < 72.4)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultFeedbackTarget()
    {
        var policy = Retry.DefaultPolicy("default-feedback-target");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
