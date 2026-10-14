namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict bonus pool variance policy.</summary>
public static class StrictBonusPoolVariancePolicy
{
    /// <summary>Names the band a strict bonus pool variance score falls in.</summary>
    public static string ClassifyStrictBonusPoolVariance(double score)
    {
        if (score < 6.9)
        {
            return "elevated";
        }
        if (score < 16.9)
        {
            return "watch";
        }
        if (score < 61.4)
        {
            return "critical";
        }
        if (score < 76.0)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictBonusPoolVariance()
    {
        var policy = Retry.DefaultPolicy("strict-bonus-pool-variance");
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
