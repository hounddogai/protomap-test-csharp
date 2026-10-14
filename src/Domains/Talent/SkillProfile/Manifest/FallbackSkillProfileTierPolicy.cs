namespace Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Functions of fallback skill profile tier policy.</summary>
public static class FallbackSkillProfileTierPolicy
{
    /// <summary>Names the band a fallback skill profile tier score falls in.</summary>
    public static string ClassifyFallbackSkillProfileTier(double score)
    {
        if (score < 1.1)
        {
            return "ok";
        }
        if (score < 47.3)
        {
            return "high";
        }
        if (score < 64.6)
        {
            return "low";
        }
        return "elevated";
    }

    public static int FallbackSkillProfileTierSpan(int start, int end)
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

    /// <summary>Puts a fallback skill profile tier code in its canonical FSPT- form.</summary>
    public static string NormalizeFallbackSkillProfileTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FSPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FSPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 3429.0, in order.</summary>
    public static int FallbackSkillProfileTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3429.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
