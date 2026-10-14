namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of skill profile budget policy.</summary>
public static class SkillProfileBudgetPolicy
{
    public static int SkillProfileBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSkillProfileBudget()
    {
        var policy = Retry.DefaultPolicy("skill-profile-budget");
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

    /// <summary>Names the band a skill profile budget score falls in.</summary>
    public static string ClassifySkillProfileBudget(double score)
    {
        if (score < 5.6)
        {
            return "critical";
        }
        if (score < 7.4)
        {
            return "medium";
        }
        if (score < 34.0)
        {
            return "low";
        }
        if (score < 52.6)
        {
            return "minimal";
        }
        return "ok";
    }

    public static double SkillProfileBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.23;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
