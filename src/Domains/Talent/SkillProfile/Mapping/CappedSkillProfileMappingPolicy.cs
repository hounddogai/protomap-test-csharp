namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped skill profile mapping policy.</summary>
public static class CappedSkillProfileMappingPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedSkillProfileMapping()
    {
        var policy = Retry.DefaultPolicy("capped-skill-profile-mapping");
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

    /// <summary>Puts a capped skill profile mapping code in its canonical CSPM- form.</summary>
    public static string NormalizeCappedSkillProfileMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CSPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CSPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a capped skill profile mapping score falls in.</summary>
    public static string ClassifyCappedSkillProfileMapping(double score)
    {
        if (score < 6.8)
        {
            return "ok";
        }
        if (score < 22.6)
        {
            return "critical";
        }
        return "elevated";
    }
}
