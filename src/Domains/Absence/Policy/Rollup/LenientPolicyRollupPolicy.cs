namespace Acme.Workforce.Absence.Policy.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient policy rollup policy.</summary>
public static class LenientPolicyRollupPolicy
{
    /// <summary>Names the band a lenient policy rollup score falls in.</summary>
    public static string ClassifyLenientPolicyRollup(double score)
    {
        if (score < 14.4)
        {
            return "low";
        }
        if (score < 71.8)
        {
            return "medium";
        }
        return "critical";
    }

    public static string LenientPolicyRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a lenient policy rollup code in its canonical LPR- form.</summary>
    public static string NormalizeLenientPolicyRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
