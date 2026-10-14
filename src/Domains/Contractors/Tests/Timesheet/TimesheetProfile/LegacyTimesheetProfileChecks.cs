namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class LegacyTimesheetProfileChecks
{
    public static void LegacyTimesheetProfileStartsInProposed()
    {
        var item = LegacyTimesheetProfile.NewLegacyTimesheetProfile("code-868", "title-921");
        Check.Equal(item.Status, LegacyTimesheetProfileStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTimesheetProfileMovesToWithdrawn()
    {
        var item = LegacyTimesheetProfile.NewLegacyTimesheetProfile("code-868", "title-921");
        try
        {
            item.MoveTo(LegacyTimesheetProfileStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTimesheetProfileStatus.Withdrawn, "status");
    }

    public static void LegacyTimesheetProfileRejectsMovesFromWithdrawn()
    {
        var item = LegacyTimesheetProfile.NewLegacyTimesheetProfile("code-868", "title-921");
        item.Status = LegacyTimesheetProfileStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyTimesheetProfileStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyTimesheetProfileStartsInProposed();
        LegacyTimesheetProfileMovesToWithdrawn();
        LegacyTimesheetProfileRejectsMovesFromWithdrawn();
    }
}
