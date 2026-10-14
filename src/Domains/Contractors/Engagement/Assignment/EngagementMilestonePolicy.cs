namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of engagement milestone policy.</summary>
public static class EngagementMilestonePolicy
{
    public static List<string> MergeEngagementMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a engagement milestone score falls in.</summary>
    public static string ClassifyEngagementMilestone(double score)
    {
        if (score < 3.5)
        {
            return "watch";
        }
        if (score < 62.3)
        {
            return "medium";
        }
        if (score < 85.9)
        {
            return "high";
        }
        if (score < 93.1)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEngagementMilestone()
    {
        var policy = Retry.DefaultPolicy("engagement-milestone");
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
