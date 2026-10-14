namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return claim policy.</summary>
public static class AssetReturnClaimPolicy
{
    public static string AssetReturnClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnClaim()
    {
        var policy = Retry.DefaultPolicy("asset-return-claim");
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
}
