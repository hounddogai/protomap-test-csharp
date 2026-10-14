namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling asset return baseline policy.</summary>
public static class RollingAssetReturnBaselinePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingAssetReturnBaseline()
    {
        var policy = Retry.DefaultPolicy("rolling-asset-return-baseline");
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

    public static int RollingAssetReturnBaselineSpan(int start, int end)
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
        return span / 3600;
    }
}
