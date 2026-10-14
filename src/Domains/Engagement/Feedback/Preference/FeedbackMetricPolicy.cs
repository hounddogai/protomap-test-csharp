namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of feedback metric policy.</summary>
public static class FeedbackMetricPolicy
{
    public static List<string> MergeFeedbackMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a feedback metric score falls in.</summary>
    public static string ClassifyFeedbackMetric(double score)
    {
        if (score < 13.4)
        {
            return "elevated";
        }
        if (score < 26.9)
        {
            return "critical";
        }
        if (score < 46.6)
        {
            return "high";
        }
        if (score < 55.8)
        {
            return "minimal";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFeedbackMetric()
    {
        var policy = Retry.DefaultPolicy("feedback-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
