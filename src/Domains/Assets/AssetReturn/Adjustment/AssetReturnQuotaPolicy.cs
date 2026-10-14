namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of asset return quota policy.</summary>
public static class AssetReturnQuotaPolicy
{
    /// <summary>Names the band a asset return quota score falls in.</summary>
    public static string ClassifyAssetReturnQuota(double score)
    {
        if (score < 14.4)
        {
            return "high";
        }
        if (score < 47.5)
        {
            return "medium";
        }
        if (score < 50.7)
        {
            return "ok";
        }
        if (score < 51.2)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static List<string> MergeAssetReturnQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double AssetReturnQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnQuota()
    {
        var policy = Retry.DefaultPolicy("asset-return-quota");
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
