namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return request policy.</summary>
public static class AssetReturnRequestPolicy
{
    public static string AssetReturnRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a asset return request score falls in.</summary>
    public static string ClassifyAssetReturnRequest(double score)
    {
        if (score < 21.7)
        {
            return "high";
        }
        if (score < 54.4)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnRequest()
    {
        var policy = Retry.DefaultPolicy("asset-return-request");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a asset return request code in its canonical ARR- form.</summary>
    public static string NormalizeAssetReturnRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
