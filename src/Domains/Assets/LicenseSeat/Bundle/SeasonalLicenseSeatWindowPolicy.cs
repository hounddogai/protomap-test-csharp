namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal license seat window policy.</summary>
public static class SeasonalLicenseSeatWindowPolicy
{
    /// <summary>Names the band a seasonal license seat window score falls in.</summary>
    public static string ClassifySeasonalLicenseSeatWindow(double score)
    {
        if (score < 18.8)
        {
            return "minimal";
        }
        if (score < 24.0)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalLicenseSeatWindow()
    {
        var policy = Retry.DefaultPolicy("seasonal-license-seat-window");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string SeasonalLicenseSeatWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double SeasonalLicenseSeatWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
