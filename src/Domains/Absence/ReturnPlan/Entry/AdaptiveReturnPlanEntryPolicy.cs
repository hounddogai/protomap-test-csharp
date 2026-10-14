namespace Acme.Workforce.Absence.ReturnPlan.Entry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive return plan entry policy.</summary>
public static class AdaptiveReturnPlanEntryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveReturnPlanEntry()
    {
        var policy = Retry.DefaultPolicy("adaptive-return-plan-entry");
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

    /// <summary>Puts a adaptive return plan entry code in its canonical ARPE- form.</summary>
    public static string NormalizeAdaptiveReturnPlanEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdaptiveReturnPlanEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
