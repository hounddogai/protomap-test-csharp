namespace Acme.Workforce.Talent.CareerPath.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback career path forecast policy.</summary>
public static class FallbackCareerPathForecastPolicy
{
    public static double FallbackCareerPathForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.88;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int FallbackCareerPathForecastSpan(int start, int end)
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
        return span / 3600;
    }
}
