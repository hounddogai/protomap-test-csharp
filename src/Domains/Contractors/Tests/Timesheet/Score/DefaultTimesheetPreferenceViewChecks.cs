namespace Acme.Workforce.Contractors.Tests.Timesheet.Score;

using Acme.Workforce.Contractors.Timesheet.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class DefaultTimesheetPreferenceViewChecks
{
    public static void DefaultTimesheetPreferenceViewShowsTheStatusInCapitals()
    {
        var item = DefaultTimesheetPreference.NewDefaultTimesheetPreference("code-49", "title-243");
        var view = DefaultTimesheetPreferenceView.ToDefaultTimesheetPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultTimesheetPreferenceView.ToDefaultTimesheetPreferenceViews(new List<DefaultTimesheetPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultTimesheetPreferenceViewShowsTheStatusInCapitals();
    }
}
