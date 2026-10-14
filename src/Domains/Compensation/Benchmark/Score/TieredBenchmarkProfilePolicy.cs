namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered benchmark profile policy.</summary>
public static class TieredBenchmarkProfilePolicy
{
    public static double TieredBenchmarkProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a tiered benchmark profile code in its canonical TBP- form.</summary>
    public static string NormalizeTieredBenchmarkProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
