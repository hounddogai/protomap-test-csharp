namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy offboarding threshold policy.</summary>
public static class LegacyOffboardingThresholdPolicy
{
    public static double LegacyOffboardingThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LegacyOffboardingThresholdSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyOffboardingThreshold()
    {
        var policy = Retry.DefaultPolicy("legacy-offboarding-threshold");
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

    /// <summary>Names the band a legacy offboarding threshold score falls in.</summary>
    public static string ClassifyLegacyOffboardingThreshold(double score)
    {
        if (score < 28.3)
        {
            return "medium";
        }
        if (score < 46.1)
        {
            return "ok";
        }
        if (score < 90.5)
        {
            return "low";
        }
        return "watch";
    }
}
