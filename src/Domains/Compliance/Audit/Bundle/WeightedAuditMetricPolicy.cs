namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted audit metric policy.</summary>
public static class WeightedAuditMetricPolicy
{
    /// <summary>Puts a weighted audit metric code in its canonical WAM- form.</summary>
    public static string NormalizeWeightedAuditMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WAM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WAM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedAuditMetric()
    {
        var policy = Retry.DefaultPolicy("weighted-audit-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int WeightedAuditMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
