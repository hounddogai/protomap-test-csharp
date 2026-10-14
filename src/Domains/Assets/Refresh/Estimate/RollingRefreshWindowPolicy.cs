namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling refresh window policy.</summary>
public static class RollingRefreshWindowPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingRefreshWindow()
    {
        var policy = Retry.DefaultPolicy("rolling-refresh-window");
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

    public static int RollingRefreshWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
