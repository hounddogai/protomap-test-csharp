namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive engagement segment policy.</summary>
public static class AdaptiveEngagementSegmentPolicy
{
    /// <summary>Names the band a adaptive engagement segment score falls in.</summary>
    public static string ClassifyAdaptiveEngagementSegment(double score)
    {
        if (score < 35.5)
        {
            return "high";
        }
        if (score < 37.6)
        {
            return "low";
        }
        if (score < 48.5)
        {
            return "watch";
        }
        return "minimal";
    }

    public static int AdaptiveEngagementSegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    public static string AdaptiveEngagementSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveEngagementSegment()
    {
        var policy = Retry.DefaultPolicy("adaptive-engagement-segment");
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
}
