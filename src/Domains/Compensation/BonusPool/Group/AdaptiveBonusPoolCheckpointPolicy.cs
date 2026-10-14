namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive bonus pool checkpoint policy.</summary>
public static class AdaptiveBonusPoolCheckpointPolicy
{
    /// <summary>Puts a adaptive bonus pool checkpoint code in its canonical ABPC- form.</summary>
    public static string NormalizeAdaptiveBonusPoolCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdaptiveBonusPoolCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveBonusPoolCheckpoint()
    {
        var policy = Retry.DefaultPolicy("adaptive-bonus-pool-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a adaptive bonus pool checkpoint score falls in.</summary>
    public static string ClassifyAdaptiveBonusPoolCheckpoint(double score)
    {
        if (score < 12.0)
        {
            return "minimal";
        }
        if (score < 50.9)
        {
            return "low";
        }
        if (score < 57.0)
        {
            return "watch";
        }
        if (score < 78.2)
        {
            return "elevated";
        }
        return "ok";
    }
}
