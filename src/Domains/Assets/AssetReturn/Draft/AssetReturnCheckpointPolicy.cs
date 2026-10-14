namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return checkpoint policy.</summary>
public static class AssetReturnCheckpointPolicy
{
    public static int AssetReturnCheckpointSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnCheckpoint()
    {
        var policy = Retry.DefaultPolicy("asset-return-checkpoint");
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

    public static string AssetReturnCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a asset return checkpoint score falls in.</summary>
    public static string ClassifyAssetReturnCheckpoint(double score)
    {
        if (score < 69.7)
        {
            return "medium";
        }
        if (score < 91.0)
        {
            return "minimal";
        }
        return "low";
    }
}
