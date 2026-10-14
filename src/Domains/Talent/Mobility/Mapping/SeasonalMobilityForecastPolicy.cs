namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal mobility forecast policy.</summary>
public static class SeasonalMobilityForecastPolicy
{
    /// <summary>Puts a seasonal mobility forecast code in its canonical SMF- form.</summary>
    public static string NormalizeSeasonalMobilityForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SMF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SMF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SeasonalMobilityForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.31;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal mobility forecast score falls in.</summary>
    public static string ClassifySeasonalMobilityForecast(double score)
    {
        if (score < 1.2)
        {
            return "low";
        }
        if (score < 17.7)
        {
            return "critical";
        }
        return "high";
    }
}
