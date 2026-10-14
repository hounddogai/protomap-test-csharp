namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantMetricChecks
{
    public static void EquityGrantMetricStartsInPlanned()
    {
        var item = EquityGrantMetric.NewEquityGrantMetric("code-770", "label-967");
        Check.Equal(item.Status, EquityGrantMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantMetricMovesToInProgress()
    {
        var item = EquityGrantMetric.NewEquityGrantMetric("code-770", "label-967");
        try
        {
            item.MoveTo(EquityGrantMetricStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantMetricStatus.InProgress, "status");
    }

    public static void EquityGrantMetricRejectsMovesFromCancelled()
    {
        var item = EquityGrantMetric.NewEquityGrantMetric("code-770", "label-967");
        item.Status = EquityGrantMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(EquityGrantMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        EquityGrantMetricStartsInPlanned();
        EquityGrantMetricMovesToInProgress();
        EquityGrantMetricRejectsMovesFromCancelled();
    }
}
