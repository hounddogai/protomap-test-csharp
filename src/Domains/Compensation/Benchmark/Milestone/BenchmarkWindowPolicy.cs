namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of benchmark window policy.</summary>
public static class BenchmarkWindowPolicy
{
    public static int BenchmarkWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a benchmark window code in its canonical BW- form.</summary>
    public static string NormalizeBenchmarkWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeBenchmarkWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
