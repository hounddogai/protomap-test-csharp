namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of offboarding override policy.</summary>
public static class OffboardingOverridePolicy
{
    /// <summary>Names the band a offboarding override score falls in.</summary>
    public static string ClassifyOffboardingOverride(double score)
    {
        if (score < 58.2)
        {
            return "high";
        }
        if (score < 93.6)
        {
            return "low";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOffboardingOverride()
    {
        var policy = Retry.DefaultPolicy("offboarding-override");
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
}
