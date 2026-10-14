namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of skill profile audit policy.</summary>
public static class SkillProfileAuditPolicy
{
    /// <summary>Names the band a skill profile audit score falls in.</summary>
    public static string ClassifySkillProfileAudit(double score)
    {
        if (score < 29.0)
        {
            return "ok";
        }
        if (score < 44.5)
        {
            return "low";
        }
        if (score < 93.3)
        {
            return "high";
        }
        return "elevated";
    }

    public static double SkillProfileAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.81;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSkillProfileAudit()
    {
        var policy = Retry.DefaultPolicy("skill-profile-audit");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
