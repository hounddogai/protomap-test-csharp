namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of offboarding baseline policy.</summary>
public static class OffboardingBaselinePolicy
{
    public static string OffboardingBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOffboardingBaseline()
    {
        var policy = Retry.DefaultPolicy("offboarding-baseline");
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

    /// <summary>Names the band a offboarding baseline score falls in.</summary>
    public static string ClassifyOffboardingBaseline(double score)
    {
        if (score < 19.4)
        {
            return "ok";
        }
        if (score < 27.9)
        {
            return "low";
        }
        if (score < 57.9)
        {
            return "medium";
        }
        return "critical";
    }
}
