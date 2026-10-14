namespace Acme.Workforce.Talent.Pool.Estimate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pool entry policy.</summary>
public static class PoolEntryPolicy
{
    /// <summary>Puts a pool entry code in its canonical PE- form.</summary>
    public static string NormalizePoolEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolEntry()
    {
        var policy = Retry.DefaultPolicy("pool-entry");
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

    public static double PoolEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pool entry score falls in.</summary>
    public static string ClassifyPoolEntry(double score)
    {
        if (score < 5.6)
        {
            return "low";
        }
        if (score < 80.2)
        {
            return "watch";
        }
        if (score < 95.7)
        {
            return "high";
        }
        return "critical";
    }
}
