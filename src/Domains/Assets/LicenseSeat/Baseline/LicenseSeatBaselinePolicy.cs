namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of license seat baseline policy.</summary>
public static class LicenseSeatBaselinePolicy
{
    /// <summary>Names the band a license seat baseline score falls in.</summary>
    public static string ClassifyLicenseSeatBaseline(double score)
    {
        if (score < 2.7)
        {
            return "critical";
        }
        if (score < 27.0)
        {
            return "ok";
        }
        if (score < 40.4)
        {
            return "high";
        }
        return "watch";
    }

    public static double LicenseSeatBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.05;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLicenseSeatBaseline()
    {
        var policy = Retry.DefaultPolicy("license-seat-baseline");
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

    public static string LicenseSeatBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
