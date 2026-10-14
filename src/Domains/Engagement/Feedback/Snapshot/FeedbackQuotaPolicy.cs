namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of feedback quota policy.</summary>
public static class FeedbackQuotaPolicy
{
    /// <summary>Puts a feedback quota code in its canonical FQ- form.</summary>
    public static string NormalizeFeedbackQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFeedbackQuota()
    {
        var policy = Retry.DefaultPolicy("feedback-quota");
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

    public static int FeedbackQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
