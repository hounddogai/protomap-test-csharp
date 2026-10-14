namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped offboarding threshold policy.</summary>
public static class CappedOffboardingThresholdPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedOffboardingThreshold()
    {
        var policy = Retry.DefaultPolicy("capped-offboarding-threshold");
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

    /// <summary>Names the band a capped offboarding threshold score falls in.</summary>
    public static string ClassifyCappedOffboardingThreshold(double score)
    {
        if (score < 50.7)
        {
            return "elevated";
        }
        if (score < 59.4)
        {
            return "medium";
        }
        if (score < 71.9)
        {
            return "critical";
        }
        if (score < 87.8)
        {
            return "high";
        }
        return "ok";
    }
}
