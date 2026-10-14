namespace Acme.Workforce.Talent.SkillProfile.Summary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional skill profile summary policy.</summary>
public static class RegionalSkillProfileSummaryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalSkillProfileSummary()
    {
        var policy = Retry.DefaultPolicy("regional-skill-profile-summary");
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

    /// <summary>Puts a regional skill profile summary code in its canonical RSPS- form.</summary>
    public static string NormalizeRegionalSkillProfileSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RSPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RSPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a regional skill profile summary score falls in.</summary>
    public static string ClassifyRegionalSkillProfileSummary(double score)
    {
        if (score < 10.3)
        {
            return "watch";
        }
        if (score < 57.5)
        {
            return "low";
        }
        if (score < 60.1)
        {
            return "medium";
        }
        return "ok";
    }
}
