namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global engagement score policy.</summary>
public static class GlobalEngagementScorePolicy
{
    /// <summary>Names the band a global engagement score score falls in.</summary>
    public static string ClassifyGlobalEngagementScore(double score)
    {
        if (score < 28.7)
        {
            return "minimal";
        }
        if (score < 58.7)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalEngagementScore()
    {
        var policy = Retry.DefaultPolicy("global-engagement-score");
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

    public static double GlobalEngagementScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.9;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeGlobalEngagementScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
