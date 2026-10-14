namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of offboarding threshold policy.</summary>
public static class OffboardingThresholdPolicy
{
    /// <summary>Names the band a offboarding threshold score falls in.</summary>
    public static string ClassifyOffboardingThreshold(double score)
    {
        if (score < 12.0)
        {
            return "medium";
        }
        if (score < 28.3)
        {
            return "critical";
        }
        if (score < 53.3)
        {
            return "ok";
        }
        if (score < 70.1)
        {
            return "low";
        }
        return "high";
    }

    public static double OffboardingThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.21;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string OffboardingThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOffboardingThreshold()
    {
        var policy = Retry.DefaultPolicy("offboarding-threshold");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
