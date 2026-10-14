namespace Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Functions of regional holiday adjustment policy.</summary>
public static class RegionalHolidayAdjustmentPolicy
{
    /// <summary>Puts a regional holiday adjustment code in its canonical RHA- form.</summary>
    public static string NormalizeRegionalHolidayAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RHA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RHA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a regional holiday adjustment score falls in.</summary>
    public static string ClassifyRegionalHolidayAdjustment(double score)
    {
        if (score < 12.9)
        {
            return "high";
        }
        if (score < 36.6)
        {
            return "watch";
        }
        if (score < 80.5)
        {
            return "elevated";
        }
        return "ok";
    }
}
