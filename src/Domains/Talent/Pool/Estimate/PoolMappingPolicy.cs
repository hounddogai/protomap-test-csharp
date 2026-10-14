namespace Acme.Workforce.Talent.Pool.Estimate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pool mapping policy.</summary>
public static class PoolMappingPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolMapping()
    {
        var policy = Retry.DefaultPolicy("pool-mapping");
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

    /// <summary>Puts a pool mapping code in its canonical PM- form.</summary>
    public static string NormalizePoolMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PoolMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
