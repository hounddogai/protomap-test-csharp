namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling benchmark adjustment policy.</summary>
public static class RollingBenchmarkAdjustmentPolicy
{
    /// <summary>Puts a rolling benchmark adjustment code in its canonical RBA- form.</summary>
    public static string NormalizeRollingBenchmarkAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RBA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RBA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RollingBenchmarkAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
