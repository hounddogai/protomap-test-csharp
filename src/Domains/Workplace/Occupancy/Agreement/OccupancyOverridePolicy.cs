namespace Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Functions of occupancy override policy.</summary>
public static class OccupancyOverridePolicy
{
    /// <summary>Names the band a occupancy override score falls in.</summary>
    public static string ClassifyOccupancyOverride(double score)
    {
        if (score < 28.3)
        {
            return "high";
        }
        if (score < 59.8)
        {
            return "ok";
        }
        if (score < 61.9)
        {
            return "watch";
        }
        if (score < 62.2)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Puts a occupancy override code in its canonical OO- form.</summary>
    public static string NormalizeOccupancyOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
