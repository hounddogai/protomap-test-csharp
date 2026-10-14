namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal holiday claim policy.</summary>
public static class SeasonalHolidayClaimPolicy
{
    public static string SeasonalHolidayClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalHolidayClaim()
    {
        var policy = Retry.DefaultPolicy("seasonal-holiday-claim");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a seasonal holiday claim score falls in.</summary>
    public static string ClassifySeasonalHolidayClaim(double score)
    {
        if (score < 62.4)
        {
            return "high";
        }
        if (score < 80.0)
        {
            return "elevated";
        }
        return "ok";
    }

    public static double SeasonalHolidayClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
