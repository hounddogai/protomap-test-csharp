namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of benchmark group policy.</summary>
public static class BenchmarkGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 32911.0, in order.</summary>
    public static int BenchmarkGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 32911.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBenchmarkGroup()
    {
        var policy = Retry.DefaultPolicy("benchmark-group");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a benchmark group score falls in.</summary>
    public static string ClassifyBenchmarkGroup(double score)
    {
        if (score < 6.1)
        {
            return "critical";
        }
        if (score < 47.7)
        {
            return "ok";
        }
        if (score < 86.2)
        {
            return "medium";
        }
        if (score < 91.4)
        {
            return "elevated";
        }
        return "watch";
    }

    public static int BenchmarkGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
