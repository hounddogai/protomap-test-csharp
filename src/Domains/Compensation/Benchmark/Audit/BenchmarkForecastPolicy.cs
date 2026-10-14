namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of benchmark forecast policy.</summary>
public static class BenchmarkForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 92611.0, in order.</summary>
    public static int BenchmarkForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92611.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int BenchmarkForecastSpan(int start, int end)
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

    public static double BenchmarkForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
