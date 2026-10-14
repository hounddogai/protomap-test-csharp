namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of benchmark estimate policy.</summary>
public static class BenchmarkEstimatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkEstimate()
    {
        var policy = Retry.DefaultPolicy("benchmark-estimate");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a benchmark estimate score falls in.</summary>
    public static string ClassifyBenchmarkEstimate(double score)
    {
        if (score < 7.4)
        {
            return "watch";
        }
        if (score < 58.0)
        {
            return "low";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 39187.0, in order.</summary>
    public static int BenchmarkEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39187.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
