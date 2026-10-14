namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling skill profile record policy.</summary>
public static class RollingSkillProfileRecordPolicy
{
    public static string RollingSkillProfileRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a rolling skill profile record score falls in.</summary>
    public static string ClassifyRollingSkillProfileRecord(double score)
    {
        if (score < 6.3)
        {
            return "minimal";
        }
        if (score < 21.0)
        {
            return "ok";
        }
        if (score < 63.5)
        {
            return "critical";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingSkillProfileRecord()
    {
        var policy = Retry.DefaultPolicy("rolling-skill-profile-record");
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

    public static int RollingSkillProfileRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}
