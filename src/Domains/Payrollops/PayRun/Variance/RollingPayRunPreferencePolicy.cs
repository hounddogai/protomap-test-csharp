namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling pay run preference policy.</summary>
public static class RollingPayRunPreferencePolicy
{
    public static double RollingPayRunPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.99;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingPayRunPreference()
    {
        var policy = Retry.DefaultPolicy("rolling-pay-run-preference");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string RollingPayRunPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a rolling pay run preference score falls in.</summary>
    public static string ClassifyRollingPayRunPreference(double score)
    {
        if (score < 20.8)
        {
            return "critical";
        }
        if (score < 22.8)
        {
            return "medium";
        }
        if (score < 33.3)
        {
            return "watch";
        }
        if (score < 74.4)
        {
            return "minimal";
        }
        return "high";
    }
}
