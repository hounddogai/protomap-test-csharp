namespace Acme.Workforce.Assets.Tests.Refresh.Metric;

using Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RefreshMetricChecks
{
    public static void RefreshMetricStartsInIdle()
    {
        var item = RefreshMetric.NewRefreshMetric("code-871", "name-310");
        Check.Equal(item.Status, RefreshMetricStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshMetricMovesToWarming()
    {
        var item = RefreshMetric.NewRefreshMetric("code-871", "name-310");
        try
        {
            item.MoveTo(RefreshMetricStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshMetricStatus.Warming, "status");
    }

    public static void RefreshMetricRejectsMovesFromStopped()
    {
        var item = RefreshMetric.NewRefreshMetric("code-871", "name-310");
        item.Status = RefreshMetricStatus.Stopped;
        Check.Fails(() => item.MoveTo(RefreshMetricStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RefreshMetricStartsInIdle();
        RefreshMetricMovesToWarming();
        RefreshMetricRejectsMovesFromStopped();
    }
}
