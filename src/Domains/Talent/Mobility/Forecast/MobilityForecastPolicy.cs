namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility forecast policy.</summary>
public static class MobilityForecastPolicy
{
    public static string MobilityForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int MobilityForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
