namespace Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Functions of career path forecast policy.</summary>
public static class CareerPathForecastPolicy
{
    /// <summary>Names the band a career path forecast score falls in.</summary>
    public static string ClassifyCareerPathForecast(double score)
    {
        if (score < 8.5)
        {
            return "watch";
        }
        if (score < 47.9)
        {
            return "high";
        }
        return "critical";
    }

    public static int CareerPathForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
