namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict feedback quota policy.</summary>
public static class StrictFeedbackQuotaPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictFeedbackQuota()
    {
        var policy = Retry.DefaultPolicy("strict-feedback-quota");
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

    public static string StrictFeedbackQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeStrictFeedbackQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a strict feedback quota score falls in.</summary>
    public static string ClassifyStrictFeedbackQuota(double score)
    {
        if (score < 86.8)
        {
            return "high";
        }
        if (score < 98.5)
        {
            return "critical";
        }
        return "low";
    }
}
