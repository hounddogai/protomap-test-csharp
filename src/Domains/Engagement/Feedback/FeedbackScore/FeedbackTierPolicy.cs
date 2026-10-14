namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of feedback tier policy.</summary>
public static class FeedbackTierPolicy
{
    public static double FeedbackTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.47;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFeedbackTier()
    {
        var policy = Retry.DefaultPolicy("feedback-tier");
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

    /// <summary>Names the band a feedback tier score falls in.</summary>
    public static string ClassifyFeedbackTier(double score)
    {
        if (score < 7.3)
        {
            return "ok";
        }
        if (score < 24.6)
        {
            return "low";
        }
        if (score < 76.8)
        {
            return "elevated";
        }
        if (score < 85.8)
        {
            return "critical";
        }
        return "high";
    }

    public static List<string> MergeFeedbackTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
