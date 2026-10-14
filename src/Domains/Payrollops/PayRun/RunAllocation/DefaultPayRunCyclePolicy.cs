namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default pay run cycle policy.</summary>
public static class DefaultPayRunCyclePolicy
{
    public static int DefaultPayRunCycleSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a default pay run cycle score falls in.</summary>
    public static string ClassifyDefaultPayRunCycle(double score)
    {
        if (score < 6.8)
        {
            return "ok";
        }
        if (score < 44.2)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultPayRunCycle()
    {
        var policy = Retry.DefaultPolicy("default-pay-run-cycle");
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
