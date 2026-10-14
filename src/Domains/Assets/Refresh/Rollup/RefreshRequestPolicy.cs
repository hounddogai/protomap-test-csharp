namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of refresh request policy.</summary>
public static class RefreshRequestPolicy
{
    /// <summary>Names the band a refresh request score falls in.</summary>
    public static string ClassifyRefreshRequest(double score)
    {
        if (score < 14.9)
        {
            return "low";
        }
        if (score < 37.0)
        {
            return "minimal";
        }
        if (score < 48.5)
        {
            return "high";
        }
        return "elevated";
    }

    public static string RefreshRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRefreshRequest()
    {
        var policy = Retry.DefaultPolicy("refresh-request");
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
