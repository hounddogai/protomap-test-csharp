namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback offboarding allocation policy.</summary>
public static class FallbackOffboardingAllocationPolicy
{
    public static int FallbackOffboardingAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a fallback offboarding allocation code in its canonical FOA- form.</summary>
    public static string NormalizeFallbackOffboardingAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FOA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FOA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackOffboardingAllocation()
    {
        var policy = Retry.DefaultPolicy("fallback-offboarding-allocation");
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
