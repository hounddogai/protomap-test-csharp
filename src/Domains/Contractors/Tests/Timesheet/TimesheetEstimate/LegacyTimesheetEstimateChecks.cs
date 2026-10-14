namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetEstimate;

using Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

/// <summary>Checks of timesheet estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetEstimateChecks
{
    public static void LegacyTimesheetEstimateStartsInPlanned()
    {
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("code-889", "title-474");
        Check.Equal(item.Status, LegacyTimesheetEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTimesheetEstimateMovesToInProgress()
    {
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("code-889", "title-474");
        try
        {
            item.MoveTo(LegacyTimesheetEstimateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTimesheetEstimateStatus.InProgress, "status");
    }

    public static void LegacyTimesheetEstimateRejectsMovesFromCancelled()
    {
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("code-889", "title-474");
        item.Status = LegacyTimesheetEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyTimesheetEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyTimesheetEstimateStartsInPlanned();
        LegacyTimesheetEstimateMovesToInProgress();
        LegacyTimesheetEstimateRejectsMovesFromCancelled();
    }
}
