namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted benchmark mapping policy.</summary>
public static class WeightedBenchmarkMappingPolicy
{
    public static double WeightedBenchmarkMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string WeightedBenchmarkMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a weighted benchmark mapping code in its canonical WBM- form.</summary>
    public static string NormalizeWeightedBenchmarkMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WBM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WBM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 10966.0, in order.</summary>
    public static int WeightedBenchmarkMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10966.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
