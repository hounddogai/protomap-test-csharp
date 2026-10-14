namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return policy policy.</summary>
public static class AssetReturnPolicyPolicy
{
    public static int AssetReturnPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a asset return policy score falls in.</summary>
    public static string ClassifyAssetReturnPolicy(double score)
    {
        if (score < 23.7)
        {
            return "critical";
        }
        if (score < 34.1)
        {
            return "watch";
        }
        if (score < 58.8)
        {
            return "high";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnPolicy()
    {
        var policy = Retry.DefaultPolicy("asset-return-policy");
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
