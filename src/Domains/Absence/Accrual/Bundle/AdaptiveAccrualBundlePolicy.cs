namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive accrual bundle policy.</summary>
public static class AdaptiveAccrualBundlePolicy
{
    /// <summary>Puts a adaptive accrual bundle code in its canonical AAB- form.</summary>
    public static string NormalizeAdaptiveAccrualBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AAB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AAB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveAccrualBundle()
    {
        var policy = Retry.DefaultPolicy("adaptive-accrual-bundle");
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

    public static int AdaptiveAccrualBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a adaptive accrual bundle score falls in.</summary>
    public static string ClassifyAdaptiveAccrualBundle(double score)
    {
        if (score < 29.7)
        {
            return "low";
        }
        if (score < 34.8)
        {
            return "watch";
        }
        return "elevated";
    }
}
