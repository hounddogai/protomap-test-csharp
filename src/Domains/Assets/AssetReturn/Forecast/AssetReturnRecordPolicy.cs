namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return record policy.</summary>
public static class AssetReturnRecordPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnRecord()
    {
        var policy = Retry.DefaultPolicy("asset-return-record");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a asset return record score falls in.</summary>
    public static string ClassifyAssetReturnRecord(double score)
    {
        if (score < 29.5)
        {
            return "elevated";
        }
        if (score < 33.0)
        {
            return "low";
        }
        if (score < 69.3)
        {
            return "watch";
        }
        if (score < 71.2)
        {
            return "critical";
        }
        return "ok";
    }
}
