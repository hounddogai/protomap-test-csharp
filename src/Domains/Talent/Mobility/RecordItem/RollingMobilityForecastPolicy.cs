namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling mobility forecast policy.</summary>
public static class RollingMobilityForecastPolicy
{
    public static string RollingMobilityForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a rolling mobility forecast score falls in.</summary>
    public static string ClassifyRollingMobilityForecast(double score)
    {
        if (score < 42.3)
        {
            return "ok";
        }
        if (score < 73.3)
        {
            return "high";
        }
        return "watch";
    }
}
