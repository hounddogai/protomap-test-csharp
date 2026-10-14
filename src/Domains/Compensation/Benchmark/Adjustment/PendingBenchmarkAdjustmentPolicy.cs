namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending benchmark adjustment policy.</summary>
public static class PendingBenchmarkAdjustmentPolicy
{
    /// <summary>Names the band a pending benchmark adjustment score falls in.</summary>
    public static string ClassifyPendingBenchmarkAdjustment(double score)
    {
        if (score < 15.2)
        {
            return "watch";
        }
        if (score < 35.7)
        {
            return "high";
        }
        if (score < 72.2)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 56985.0, in order.</summary>
    public static int PendingBenchmarkAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56985.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingBenchmarkAdjustment()
    {
        var policy = Retry.DefaultPolicy("pending-benchmark-adjustment");
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
}
