namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict benchmark audit policy.</summary>
public static class StrictBenchmarkAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 74155.0, in order.</summary>
    public static int StrictBenchmarkAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74155.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictBenchmarkAudit()
    {
        var policy = Retry.DefaultPolicy("strict-benchmark-audit");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
