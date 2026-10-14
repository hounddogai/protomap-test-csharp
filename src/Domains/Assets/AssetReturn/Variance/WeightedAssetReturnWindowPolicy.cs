namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted asset return window policy.</summary>
public static class WeightedAssetReturnWindowPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedAssetReturnWindow()
    {
        var policy = Retry.DefaultPolicy("weighted-asset-return-window");
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

    /// <summary>Names the band a weighted asset return window score falls in.</summary>
    public static string ClassifyWeightedAssetReturnWindow(double score)
    {
        if (score < 14.6)
        {
            return "medium";
        }
        if (score < 27.8)
        {
            return "watch";
        }
        if (score < 39.9)
        {
            return "high";
        }
        return "elevated";
    }
}
