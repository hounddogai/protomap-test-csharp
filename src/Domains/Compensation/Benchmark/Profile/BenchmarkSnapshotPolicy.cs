namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of benchmark snapshot policy.</summary>
public static class BenchmarkSnapshotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 49890.0, in order.</summary>
    public static int BenchmarkSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49890.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeBenchmarkSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a benchmark snapshot code in its canonical BS- form.</summary>
    public static string NormalizeBenchmarkSnapshotCode(string code)
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
