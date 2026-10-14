namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional refresh request policy.</summary>
public static class RegionalRefreshRequestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalRefreshRequest()
    {
        var policy = Retry.DefaultPolicy("regional-refresh-request");
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

    public static int RegionalRefreshRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
