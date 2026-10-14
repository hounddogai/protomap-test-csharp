namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive engagement notice policy.</summary>
public static class AdaptiveEngagementNoticePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveEngagementNotice()
    {
        var policy = Retry.DefaultPolicy("adaptive-engagement-notice");
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

    /// <summary>Names the band a adaptive engagement notice score falls in.</summary>
    public static string ClassifyAdaptiveEngagementNotice(double score)
    {
        if (score < 24.5)
        {
            return "elevated";
        }
        if (score < 33.5)
        {
            return "low";
        }
        return "minimal";
    }
}
