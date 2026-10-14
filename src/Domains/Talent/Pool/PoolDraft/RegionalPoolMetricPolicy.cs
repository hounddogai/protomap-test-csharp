namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional pool metric policy.</summary>
public static class RegionalPoolMetricPolicy
{
    public static List<string> MergeRegionalPoolMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalPoolMetric()
    {
        var policy = Retry.DefaultPolicy("regional-pool-metric");
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

    /// <summary>Puts a regional pool metric code in its canonical RPM- form.</summary>
    public static string NormalizeRegionalPoolMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a regional pool metric score falls in.</summary>
    public static string ClassifyRegionalPoolMetric(double score)
    {
        if (score < 34.3)
        {
            return "elevated";
        }
        if (score < 67.5)
        {
            return "high";
        }
        if (score < 91.6)
        {
            return "low";
        }
        return "watch";
    }
}
