namespace Acme.Workforce.Compensation.Benchmark.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered benchmark summary policy.</summary>
public static class TieredBenchmarkSummaryPolicy
{
    public static List<string> MergeTieredBenchmarkSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tiered benchmark summary score falls in.</summary>
    public static string ClassifyTieredBenchmarkSummary(double score)
    {
        if (score < 49.6)
        {
            return "medium";
        }
        if (score < 90.7)
        {
            return "minimal";
        }
        return "elevated";
    }

    /// <summary>Puts a tiered benchmark summary code in its canonical TBS- form.</summary>
    public static string NormalizeTieredBenchmarkSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TBS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TBS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 81786.0, in order.</summary>
    public static int TieredBenchmarkSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81786.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
