namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of engagement group policy.</summary>
public static class EngagementGroupPolicy
{
    public static string EngagementGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEngagementGroup()
    {
        var policy = Retry.DefaultPolicy("engagement-group");
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

    /// <summary>Names the band a engagement group score falls in.</summary>
    public static string ClassifyEngagementGroup(double score)
    {
        if (score < 57.4)
        {
            return "medium";
        }
        if (score < 63.0)
        {
            return "low";
        }
        if (score < 78.6)
        {
            return "watch";
        }
        return "high";
    }
}
