namespace Acme.Workforce.Contractors.Tests.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingTimesheetMetricChecks
{
    public static void PendingTimesheetMetricStartsInRequested()
    {
        var item = PendingTimesheetMetric.NewPendingTimesheetMetric("code-342", "label-624");
        Check.Equal(item.Status, PendingTimesheetMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingTimesheetMetricMovesToGranted()
    {
        var item = PendingTimesheetMetric.NewPendingTimesheetMetric("code-342", "label-624");
        try
        {
            item.MoveTo(PendingTimesheetMetricStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingTimesheetMetricStatus.Granted, "status");
    }

    public static void PendingTimesheetMetricRejectsMovesFromExpired()
    {
        var item = PendingTimesheetMetric.NewPendingTimesheetMetric("code-342", "label-624");
        item.Status = PendingTimesheetMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingTimesheetMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingTimesheetMetricStartsInRequested();
        PendingTimesheetMetricMovesToGranted();
        PendingTimesheetMetricRejectsMovesFromExpired();
    }
}
