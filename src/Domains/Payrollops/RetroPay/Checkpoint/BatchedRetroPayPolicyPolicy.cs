namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched retro pay policy policy.</summary>
public static class BatchedRetroPayPolicyPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedRetroPayPolicy()
    {
        var policy = Retry.DefaultPolicy("batched-retro-pay-policy");
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

    /// <summary>Names the band a batched retro pay policy score falls in.</summary>
    public static string ClassifyBatchedRetroPayPolicy(double score)
    {
        if (score < 11.0)
        {
            return "low";
        }
        if (score < 25.2)
        {
            return "ok";
        }
        if (score < 54.9)
        {
            return "elevated";
        }
        return "minimal";
    }
}
