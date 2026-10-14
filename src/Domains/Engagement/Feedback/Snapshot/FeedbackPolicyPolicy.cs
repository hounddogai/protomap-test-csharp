namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of feedback policy policy.</summary>
public static class FeedbackPolicyPolicy
{
    /// <summary>Puts a feedback policy code in its canonical FP- form.</summary>
    public static string NormalizeFeedbackPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int FeedbackPolicySpan(int start, int end)
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
    public static int ProbeFeedbackPolicy()
    {
        var policy = Retry.DefaultPolicy("feedback-policy");
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

    public static double FeedbackPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
