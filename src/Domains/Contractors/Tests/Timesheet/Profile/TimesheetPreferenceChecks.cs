namespace Acme.Workforce.Contractors.Tests.Timesheet.Profile;

using Acme.Workforce.Contractors.Timesheet.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class TimesheetPreferenceChecks
{
    public static void TimesheetPreferenceStartsInRequested()
    {
        var item = TimesheetPreference.NewTimesheetPreference("code-165", "name-421");
        Check.Equal(item.Status, TimesheetPreferenceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetPreferenceMovesToRevoked()
    {
        var item = TimesheetPreference.NewTimesheetPreference("code-165", "name-421");
        try
        {
            item.MoveTo(TimesheetPreferenceStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetPreferenceStatus.Revoked, "status");
    }

    public static void TimesheetPreferenceRejectsMovesFromExpired()
    {
        var item = TimesheetPreference.NewTimesheetPreference("code-165", "name-421");
        item.Status = TimesheetPreferenceStatus.Expired;
        Check.Fails(() => item.MoveTo(TimesheetPreferenceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TimesheetPreferenceStartsInRequested();
        TimesheetPreferenceMovesToRevoked();
        TimesheetPreferenceRejectsMovesFromExpired();
    }
}
