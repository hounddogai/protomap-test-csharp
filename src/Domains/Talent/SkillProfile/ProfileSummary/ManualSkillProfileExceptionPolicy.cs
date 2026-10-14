namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual skill profile exception policy.</summary>
public static class ManualSkillProfileExceptionPolicy
{
    /// <summary>Names the band a manual skill profile exception score falls in.</summary>
    public static string ClassifyManualSkillProfileException(double score)
    {
        if (score < 31.2)
        {
            return "ok";
        }
        if (score < 36.4)
        {
            return "low";
        }
        if (score < 79.6)
        {
            return "high";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualSkillProfileException()
    {
        var policy = Retry.DefaultPolicy("manual-skill-profile-exception");
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

    public static double ManualSkillProfileExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
