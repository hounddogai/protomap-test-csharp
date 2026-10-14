namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PrimaryTimesheetGroupChecks
{
    public static void PrimaryTimesheetGroupStartsInDraft()
    {
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("code-823", "name-552");
        Check.Equal(item.Status, PrimaryTimesheetGroupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryTimesheetGroupMovesToActive()
    {
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("code-823", "name-552");
        try
        {
            item.MoveTo(PrimaryTimesheetGroupStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryTimesheetGroupStatus.Active, "status");
    }

    public static void PrimaryTimesheetGroupRejectsMovesFromClosed()
    {
        var item = PrimaryTimesheetGroup.NewPrimaryTimesheetGroup("code-823", "name-552");
        item.Status = PrimaryTimesheetGroupStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryTimesheetGroupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryTimesheetGroupStartsInDraft();
        PrimaryTimesheetGroupMovesToActive();
        PrimaryTimesheetGroupRejectsMovesFromClosed();
    }
}
