namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional skill profile manifest policy.</summary>
public static class RegionalSkillProfileManifestPolicy
{
    /// <summary>Puts a regional skill profile manifest code in its canonical RSPM- form.</summary>
    public static string NormalizeRegionalSkillProfileManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RSPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RSPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalSkillProfileManifest()
    {
        var policy = Retry.DefaultPolicy("regional-skill-profile-manifest");
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

    /// <summary>Counts how many amounts fit in a budget of 53223.0, in order.</summary>
    public static int RegionalSkillProfileManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53223.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
