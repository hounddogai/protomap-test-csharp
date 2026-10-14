namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of benchmark audit policy.</summary>
public static class BenchmarkAuditPolicy
{
    /// <summary>Puts a benchmark audit code in its canonical BA- form.</summary>
    public static string NormalizeBenchmarkAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeBenchmarkAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double BenchmarkAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
