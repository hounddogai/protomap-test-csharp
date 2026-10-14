namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy career path digest policy.</summary>
public static class LegacyCareerPathDigestPolicy
{
    /// <summary>Names the band a legacy career path digest score falls in.</summary>
    public static string ClassifyLegacyCareerPathDigest(double score)
    {
        if (score < 31.6)
        {
            return "high";
        }
        if (score < 86.0)
        {
            return "minimal";
        }
        return "medium";
    }

    public static int LegacyCareerPathDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static string LegacyCareerPathDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a legacy career path digest code in its canonical LCPD- form.</summary>
    public static string NormalizeLegacyCareerPathDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LCPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LCPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
