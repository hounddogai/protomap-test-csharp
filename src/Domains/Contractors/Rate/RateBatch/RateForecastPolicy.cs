namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rate forecast policy.</summary>
public static class RateForecastPolicy
{
    public static string RateForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a rate forecast score falls in.</summary>
    public static string ClassifyRateForecast(double score)
    {
        if (score < 9.9)
        {
            return "critical";
        }
        if (score < 31.7)
        {
            return "ok";
        }
        if (score < 74.2)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static int RateForecastSpan(int start, int end)
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

    public static double RateForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
