namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict offboarding override policy.</summary>
public static class StrictOffboardingOverridePolicy
{
    /// <summary>Puts a strict offboarding override code in its canonical SOO- form.</summary>
    public static string NormalizeStrictOffboardingOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictOffboardingOverride()
    {
        var policy = Retry.DefaultPolicy("strict-offboarding-override");
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
