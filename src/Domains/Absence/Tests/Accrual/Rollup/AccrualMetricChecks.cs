namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccrualMetricChecks
{
    public static void AccrualMetricStartsInPlanned()
    {
        var item = AccrualMetric.NewAccrualMetric("code-885", "label-3");
        Check.Equal(item.Status, AccrualMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualMetricMovesToScheduled()
    {
        var item = AccrualMetric.NewAccrualMetric("code-885", "label-3");
        try
        {
            item.MoveTo(AccrualMetricStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualMetricStatus.Scheduled, "status");
    }

    public static void AccrualMetricRejectsMovesFromCancelled()
    {
        var item = AccrualMetric.NewAccrualMetric("code-885", "label-3");
        item.Status = AccrualMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AccrualMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AccrualMetricStartsInPlanned();
        AccrualMetricMovesToScheduled();
        AccrualMetricRejectsMovesFromCancelled();
    }
}
