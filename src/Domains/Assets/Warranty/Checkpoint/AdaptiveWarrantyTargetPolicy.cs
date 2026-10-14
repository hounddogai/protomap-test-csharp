namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive warranty target policy.</summary>
public static class AdaptiveWarrantyTargetPolicy
{
    /// <summary>Names the band a adaptive warranty target score falls in.</summary>
    public static string ClassifyAdaptiveWarrantyTarget(double score)
    {
        if (score < 36.8)
        {
            return "ok";
        }
        if (score < 41.7)
        {
            return "elevated";
        }
        if (score < 45.0)
        {
            return "high";
        }
        if (score < 87.6)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveWarrantyTarget()
    {
        var policy = Retry.DefaultPolicy("adaptive-warranty-target");
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
}
