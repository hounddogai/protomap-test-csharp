namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled pay band forecast policy.</summary>
public static class ScheduledPayBandForecastPolicy
{
    public static double ScheduledPayBandForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.19;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int ScheduledPayBandForecastSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a scheduled pay band forecast score falls in.</summary>
    public static string ClassifyScheduledPayBandForecast(double score)
    {
        if (score < 4.5)
        {
            return "medium";
        }
        if (score < 6.6)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledPayBandForecast()
    {
        var policy = Retry.DefaultPolicy("scheduled-pay-band-forecast");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
