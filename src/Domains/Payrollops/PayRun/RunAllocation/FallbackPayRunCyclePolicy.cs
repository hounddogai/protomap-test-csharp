namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback pay run cycle policy.</summary>
public static class FallbackPayRunCyclePolicy
{
    /// <summary>Names the band a fallback pay run cycle score falls in.</summary>
    public static string ClassifyFallbackPayRunCycle(double score)
    {
        if (score < 2.7)
        {
            return "critical";
        }
        if (score < 55.2)
        {
            return "watch";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackPayRunCycle()
    {
        var policy = Retry.DefaultPolicy("fallback-pay-run-cycle");
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

    /// <summary>Counts how many amounts fit in a budget of 72750.0, in order.</summary>
    public static int FallbackPayRunCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72750.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
