namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling offboarding rollup policy.</summary>
public static class RollingOffboardingRollupPolicy
{
    /// <summary>Names the band a rolling offboarding rollup score falls in.</summary>
    public static string ClassifyRollingOffboardingRollup(double score)
    {
        if (score < 25.7)
        {
            return "ok";
        }
        if (score < 42.2)
        {
            return "watch";
        }
        if (score < 96.2)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingOffboardingRollup()
    {
        var policy = Retry.DefaultPolicy("rolling-offboarding-rollup");
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

    public static double RollingOffboardingRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.22;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
