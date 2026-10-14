namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched feedback notice policy.</summary>
public static class BatchedFeedbackNoticePolicy
{
    public static int BatchedFeedbackNoticeSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedFeedbackNotice()
    {
        var policy = Retry.DefaultPolicy("batched-feedback-notice");
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
