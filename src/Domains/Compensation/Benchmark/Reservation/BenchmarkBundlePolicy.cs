namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of benchmark bundle policy.</summary>
public static class BenchmarkBundlePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 10164.0, in order.</summary>
    public static int BenchmarkBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10164.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double BenchmarkBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
