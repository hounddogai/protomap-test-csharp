namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of benchmark baseline policy.</summary>
public static class BenchmarkBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 89721.0, in order.</summary>
    public static int BenchmarkBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89721.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double BenchmarkBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int BenchmarkBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a benchmark baseline code in its canonical BB- form.</summary>
    public static string NormalizeBenchmarkBaselineCode(string code)
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
