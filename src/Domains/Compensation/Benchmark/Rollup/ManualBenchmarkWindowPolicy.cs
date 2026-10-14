namespace Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Functions of manual benchmark window policy.</summary>
public static class ManualBenchmarkWindowPolicy
{
    /// <summary>Names the band a manual benchmark window score falls in.</summary>
    public static string ClassifyManualBenchmarkWindow(double score)
    {
        if (score < 29.7)
        {
            return "medium";
        }
        if (score < 72.5)
        {
            return "ok";
        }
        if (score < 74.1)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Puts a manual benchmark window code in its canonical MBW- form.</summary>
    public static string NormalizeManualBenchmarkWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MBW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MBW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
