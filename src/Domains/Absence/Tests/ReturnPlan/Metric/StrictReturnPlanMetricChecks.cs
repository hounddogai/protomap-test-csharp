namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class StrictReturnPlanMetricChecks
{
    public static void StrictReturnPlanMetricStartsInDraft()
    {
        var item = StrictReturnPlanMetric.NewStrictReturnPlanMetric("code-948", "label-509");
        Check.Equal(item.Status, StrictReturnPlanMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictReturnPlanMetricMovesToSuspended()
    {
        var item = StrictReturnPlanMetric.NewStrictReturnPlanMetric("code-948", "label-509");
        try
        {
            item.MoveTo(StrictReturnPlanMetricStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictReturnPlanMetricStatus.Suspended, "status");
    }

    public static void StrictReturnPlanMetricRejectsMovesFromClosed()
    {
        var item = StrictReturnPlanMetric.NewStrictReturnPlanMetric("code-948", "label-509");
        item.Status = StrictReturnPlanMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictReturnPlanMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictReturnPlanMetricStartsInDraft();
        StrictReturnPlanMetricMovesToSuspended();
        StrictReturnPlanMetricRejectsMovesFromClosed();
    }
}
