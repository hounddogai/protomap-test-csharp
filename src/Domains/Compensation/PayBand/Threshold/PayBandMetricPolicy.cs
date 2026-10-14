namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay band metric policy.</summary>
public static class PayBandMetricPolicy
{
    /// <summary>Names the band a pay band metric score falls in.</summary>
    public static string ClassifyPayBandMetric(double score)
    {
        if (score < 15.8)
        {
            return "low";
        }
        if (score < 44.9)
        {
            return "minimal";
        }
        if (score < 49.7)
        {
            return "medium";
        }
        if (score < 72.0)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Puts a pay band metric code in its canonical PBM- form.</summary>
    public static string NormalizePayBandMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PayBandMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandMetric()
    {
        var policy = Retry.DefaultPolicy("pay-band-metric");
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
