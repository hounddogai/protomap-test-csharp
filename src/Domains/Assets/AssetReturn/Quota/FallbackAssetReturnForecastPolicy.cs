namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback asset return forecast policy.</summary>
public static class FallbackAssetReturnForecastPolicy
{
    public static int FallbackAssetReturnForecastSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackAssetReturnForecast()
    {
        var policy = Retry.DefaultPolicy("fallback-asset-return-forecast");
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
