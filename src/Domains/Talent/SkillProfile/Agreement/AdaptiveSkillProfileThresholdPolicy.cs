namespace Acme.Workforce.Talent.SkillProfile.Agreement;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive skill profile threshold policy.</summary>
public static class AdaptiveSkillProfileThresholdPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveSkillProfileThreshold()
    {
        var policy = Retry.DefaultPolicy("adaptive-skill-profile-threshold");
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

    /// <summary>Names the band a adaptive skill profile threshold score falls in.</summary>
    public static string ClassifyAdaptiveSkillProfileThreshold(double score)
    {
        if (score < 42.8)
        {
            return "critical";
        }
        if (score < 96.1)
        {
            return "low";
        }
        return "watch";
    }

    public static string AdaptiveSkillProfileThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int AdaptiveSkillProfileThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
