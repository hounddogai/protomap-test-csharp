namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict pool forecast policy.</summary>
public static class StrictPoolForecastPolicy
{
    public static double StrictPoolForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a strict pool forecast score falls in.</summary>
    public static string ClassifyStrictPoolForecast(double score)
    {
        if (score < 58.6)
        {
            return "high";
        }
        if (score < 61.3)
        {
            return "ok";
        }
        if (score < 77.8)
        {
            return "critical";
        }
        if (score < 84.3)
        {
            return "medium";
        }
        return "watch";
    }
}
