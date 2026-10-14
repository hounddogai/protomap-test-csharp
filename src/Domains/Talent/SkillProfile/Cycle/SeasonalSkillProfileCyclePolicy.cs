namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal skill profile cycle policy.</summary>
public static class SeasonalSkillProfileCyclePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalSkillProfileCycle()
    {
        var policy = Retry.DefaultPolicy("seasonal-skill-profile-cycle");
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

    public static string SeasonalSkillProfileCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a seasonal skill profile cycle score falls in.</summary>
    public static string ClassifySeasonalSkillProfileCycle(double score)
    {
        if (score < 45.2)
        {
            return "ok";
        }
        if (score < 53.3)
        {
            return "low";
        }
        return "high";
    }

    /// <summary>Puts a seasonal skill profile cycle code in its canonical SSPC- form.</summary>
    public static string NormalizeSeasonalSkillProfileCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
