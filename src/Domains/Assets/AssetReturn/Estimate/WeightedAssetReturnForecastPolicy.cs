namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted asset return forecast policy.</summary>
public static class WeightedAssetReturnForecastPolicy
{
    /// <summary>Puts a weighted asset return forecast code in its canonical WARF- form.</summary>
    public static string NormalizeWeightedAssetReturnForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WARF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WARF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedAssetReturnForecast()
    {
        var policy = Retry.DefaultPolicy("weighted-asset-return-forecast");
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

    public static string WeightedAssetReturnForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
