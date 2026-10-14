namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy booking rollup policy.</summary>
public static class LegacyBookingRollupPolicy
{
    /// <summary>Puts a legacy booking rollup code in its canonical LBR- form.</summary>
    public static string NormalizeLegacyBookingRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LBR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LBR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a legacy booking rollup score falls in.</summary>
    public static string ClassifyLegacyBookingRollup(double score)
    {
        if (score < 30.3)
        {
            return "medium";
        }
        if (score < 31.9)
        {
            return "critical";
        }
        return "high";
    }

    public static string LegacyBookingRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
