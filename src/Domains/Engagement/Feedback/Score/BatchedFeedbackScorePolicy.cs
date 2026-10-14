namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched feedback score policy.</summary>
public static class BatchedFeedbackScorePolicy
{
    public static double BatchedFeedbackScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedFeedbackScore()
    {
        var policy = Retry.DefaultPolicy("batched-feedback-score");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
