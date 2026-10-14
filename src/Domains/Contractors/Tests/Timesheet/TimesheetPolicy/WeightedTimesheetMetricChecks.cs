namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Checks of timesheet policy code, run by the project's runner.</summary>
public static class WeightedTimesheetMetricChecks
{
    public static void WeightedTimesheetMetricStartsInIdle()
    {
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("code-227", "title-100");
        Check.Equal(item.Status, WeightedTimesheetMetricStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedTimesheetMetricMovesToWarming()
    {
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("code-227", "title-100");
        try
        {
            item.MoveTo(WeightedTimesheetMetricStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedTimesheetMetricStatus.Warming, "status");
    }

    public static void WeightedTimesheetMetricRejectsMovesFromStopped()
    {
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("code-227", "title-100");
        item.Status = WeightedTimesheetMetricStatus.Stopped;
        Check.Fails(() => item.MoveTo(WeightedTimesheetMetricStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WeightedTimesheetMetricStartsInIdle();
        WeightedTimesheetMetricMovesToWarming();
        WeightedTimesheetMetricRejectsMovesFromStopped();
    }
}
