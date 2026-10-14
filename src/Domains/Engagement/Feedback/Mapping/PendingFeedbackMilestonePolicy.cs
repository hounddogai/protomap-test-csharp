namespace Acme.Workforce.Engagement.Feedback.Mapping;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending feedback milestone policy.</summary>
public static class PendingFeedbackMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 46356.0, in order.</summary>
    public static int PendingFeedbackMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 46356.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingFeedbackMilestone()
    {
        var policy = Retry.DefaultPolicy("pending-feedback-milestone");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double PendingFeedbackMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.31;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
