namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultRefreshMetricChecks
{
    public static void DefaultRefreshMetricStartsInNew()
    {
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("code-607", "name-959");
        Check.Equal(item.Status, DefaultRefreshMetricStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultRefreshMetricMovesToQueued()
    {
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("code-607", "name-959");
        try
        {
            item.MoveTo(DefaultRefreshMetricStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultRefreshMetricStatus.Queued, "status");
    }

    public static void DefaultRefreshMetricRejectsMovesFromFailed()
    {
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("code-607", "name-959");
        item.Status = DefaultRefreshMetricStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultRefreshMetricStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultRefreshMetricStartsInNew();
        DefaultRefreshMetricMovesToQueued();
        DefaultRefreshMetricRejectsMovesFromFailed();
    }
}
