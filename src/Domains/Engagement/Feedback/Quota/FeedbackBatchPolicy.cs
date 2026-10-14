namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of feedback batch policy.</summary>
public static class FeedbackBatchPolicy
{
    public static int FeedbackBatchSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFeedbackBatch()
    {
        var policy = Retry.DefaultPolicy("feedback-batch");
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
}
