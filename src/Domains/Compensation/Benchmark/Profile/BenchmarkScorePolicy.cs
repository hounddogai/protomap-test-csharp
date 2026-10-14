namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of benchmark score policy.</summary>
public static class BenchmarkScorePolicy
{
    public static int BenchmarkScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static string BenchmarkScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a benchmark score code in its canonical BS- form.</summary>
    public static string NormalizeBenchmarkScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
