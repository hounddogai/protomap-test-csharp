namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy access badge metric policy.</summary>
public static class LegacyAccessBadgeMetricPolicy
{
    public static string LegacyAccessBadgeMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a legacy access badge metric code in its canonical LABM- form.</summary>
    public static string NormalizeLegacyAccessBadgeMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LABM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LABM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LegacyAccessBadgeMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyAccessBadgeMetric()
    {
        var policy = Retry.DefaultPolicy("legacy-access-badge-metric");
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
}
