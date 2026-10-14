namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PrimaryTimesheetGroupViewChecks
{
    public static void PrimaryTimesheetGroupViewShowsTheStatusInCapitals()
    {
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("code-823", "name-552");
        var view = PrimaryTimesheetGroupView.ToPrimaryTimesheetGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryTimesheetGroupView.ToPrimaryTimesheetGroupViews(new List<PrimaryTimesheetGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryTimesheetGroupViewShowsTheStatusInCapitals();
    }
}
