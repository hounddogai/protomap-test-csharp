namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LegacyTimesheetMetricChecks
{
    public static void LegacyTimesheetMetricStartsInProposed()
    {
        var item = LegacyTimesheetMetric.NewLegacyTimesheetMetric("code-657", "title-76");
        Check.Equal(item.Status, LegacyTimesheetMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTimesheetMetricMovesToRejected()
    {
        var item = LegacyTimesheetMetric.NewLegacyTimesheetMetric("code-657", "title-76");
        try
        {
            item.MoveTo(LegacyTimesheetMetricStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTimesheetMetricStatus.Rejected, "status");
    }

    public static void LegacyTimesheetMetricRejectsMovesFromWithdrawn()
    {
        var item = LegacyTimesheetMetric.NewLegacyTimesheetMetric("code-657", "title-76");
        item.Status = LegacyTimesheetMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyTimesheetMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyTimesheetMetricStartsInProposed();
        LegacyTimesheetMetricMovesToRejected();
        LegacyTimesheetMetricRejectsMovesFromWithdrawn();
    }
}
