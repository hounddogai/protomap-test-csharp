namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive warranty mapping policy.</summary>
public static class AdaptiveWarrantyMappingPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveWarrantyMapping()
    {
        var policy = Retry.DefaultPolicy("adaptive-warranty-mapping");
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

    /// <summary>Names the band a adaptive warranty mapping score falls in.</summary>
    public static string ClassifyAdaptiveWarrantyMapping(double score)
    {
        if (score < 6.3)
        {
            return "minimal";
        }
        if (score < 9.0)
        {
            return "critical";
        }
        if (score < 57.8)
        {
            return "high";
        }
        return "watch";
    }
}
