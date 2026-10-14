namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched pay run variance policy.</summary>
public static class BatchedPayRunVariancePolicy
{
    public static int BatchedPayRunVarianceSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedPayRunVariance()
    {
        var policy = Retry.DefaultPolicy("batched-pay-run-variance");
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
