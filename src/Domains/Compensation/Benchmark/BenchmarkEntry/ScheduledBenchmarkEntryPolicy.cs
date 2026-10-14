namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Functions of scheduled benchmark entry policy.</summary>
public static class ScheduledBenchmarkEntryPolicy
{
    /// <summary>Puts a scheduled benchmark entry code in its canonical SBE- form.</summary>
    public static string NormalizeScheduledBenchmarkEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a scheduled benchmark entry score falls in.</summary>
    public static string ClassifyScheduledBenchmarkEntry(double score)
    {
        if (score < 41.8)
        {
            return "medium";
        }
        if (score < 56.0)
        {
            return "high";
        }
        return "minimal";
    }
}
