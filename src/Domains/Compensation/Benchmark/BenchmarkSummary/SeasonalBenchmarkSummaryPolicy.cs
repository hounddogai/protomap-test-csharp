namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal benchmark summary policy.</summary>
public static class SeasonalBenchmarkSummaryPolicy
{
    /// <summary>Puts a seasonal benchmark summary code in its canonical SBS- form.</summary>
    public static string NormalizeSeasonalBenchmarkSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SeasonalBenchmarkSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 93093.0, in order.</summary>
    public static int SeasonalBenchmarkSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93093.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
