namespace Acme.Workforce.Contractors.Tests.Timesheet.Score;

using Acme.Workforce.Contractors.Timesheet.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class DefaultTimesheetPreferenceChecks
{
    public static void DefaultTimesheetPreferenceStartsInOpen()
    {
        var item = DefaultTimesheetPreference.NewDefaultTimesheetPreference("code-49", "title-243");
        Check.Equal(item.Status, DefaultTimesheetPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultTimesheetPreferenceMovesToResolved()
    {
        var item = DefaultTimesheetPreference.NewDefaultTimesheetPreference("code-49", "title-243");
        try
        {
            item.MoveTo(DefaultTimesheetPreferenceStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultTimesheetPreferenceStatus.Resolved, "status");
    }

    public static void DefaultTimesheetPreferenceRejectsMovesFromArchived()
    {
        var item = DefaultTimesheetPreference.NewDefaultTimesheetPreference("code-49", "title-243");
        item.Status = DefaultTimesheetPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultTimesheetPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultTimesheetPreferenceStartsInOpen();
        DefaultTimesheetPreferenceMovesToResolved();
        DefaultTimesheetPreferenceRejectsMovesFromArchived();
    }
}
