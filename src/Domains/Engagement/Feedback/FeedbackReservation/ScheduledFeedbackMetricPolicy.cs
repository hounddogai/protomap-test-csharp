namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled feedback metric policy.</summary>
public static class ScheduledFeedbackMetricPolicy
{
    /// <summary>Names the band a scheduled feedback metric score falls in.</summary>
    public static string ClassifyScheduledFeedbackMetric(double score)
    {
        if (score < 24.0)
        {
            return "high";
        }
        if (score < 33.0)
        {
            return "watch";
        }
        if (score < 58.4)
        {
            return "medium";
        }
        return "critical";
    }

    /// <summary>Puts a scheduled feedback metric code in its canonical SFM- form.</summary>
    public static string NormalizeScheduledFeedbackMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeScheduledFeedbackMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledFeedbackMetric()
    {
        var policy = Retry.DefaultPolicy("scheduled-feedback-metric");
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
