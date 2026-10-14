namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgMetricChecks
{
    public static void ReorgMetricStartsInDraft()
    {
        var item = ReorgMetric.NewReorgMetric("code-772", "title-38");
        Check.Equal(item.Status, ReorgMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgMetricMovesToSuspended()
    {
        var item = ReorgMetric.NewReorgMetric("code-772", "title-38");
        try
        {
            item.MoveTo(ReorgMetricStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgMetricStatus.Suspended, "status");
    }

    public static void ReorgMetricRejectsMovesFromClosed()
    {
        var item = ReorgMetric.NewReorgMetric("code-772", "title-38");
        item.Status = ReorgMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(ReorgMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ReorgMetricStartsInDraft();
        ReorgMetricMovesToSuspended();
        ReorgMetricRejectsMovesFromClosed();
    }
}
