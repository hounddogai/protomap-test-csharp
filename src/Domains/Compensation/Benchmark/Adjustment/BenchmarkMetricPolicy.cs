namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of benchmark metric policy.</summary>
public static class BenchmarkMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 96553.0, in order.</summary>
    public static int BenchmarkMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 96553.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkMetric()
    {
        var policy = Retry.DefaultPolicy("benchmark-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
