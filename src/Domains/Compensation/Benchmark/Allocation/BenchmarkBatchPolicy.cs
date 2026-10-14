namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of benchmark batch policy.</summary>
public static class BenchmarkBatchPolicy
{
    public static List<string> MergeBenchmarkBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 23376.0, in order.</summary>
    public static int BenchmarkBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 23376.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a benchmark batch code in its canonical BB- form.</summary>
    public static string NormalizeBenchmarkBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
