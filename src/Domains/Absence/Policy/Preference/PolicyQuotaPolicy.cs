namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy quota policy.</summary>
public static class PolicyQuotaPolicy
{
    /// <summary>Puts a policy quota code in its canonical PQ- form.</summary>
    public static string NormalizePolicyQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a policy quota score falls in.</summary>
    public static string ClassifyPolicyQuota(double score)
    {
        if (score < 29.2)
        {
            return "minimal";
        }
        if (score < 48.2)
        {
            return "ok";
        }
        if (score < 86.5)
        {
            return "low";
        }
        return "medium";
    }

    public static string PolicyQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
