namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalTimesheetMilestoneChecks
{
    public static void RegionalTimesheetMilestoneStartsInOpen()
    {
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("code-240", "name-362");
        Check.Equal(item.Status, RegionalTimesheetMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalTimesheetMilestoneMovesToInReview()
    {
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("code-240", "name-362");
        try
        {
            item.MoveTo(RegionalTimesheetMilestoneStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalTimesheetMilestoneStatus.InReview, "status");
    }

    public static void RegionalTimesheetMilestoneRejectsMovesFromArchived()
    {
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("code-240", "name-362");
        item.Status = RegionalTimesheetMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalTimesheetMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalTimesheetMilestoneStartsInOpen();
        RegionalTimesheetMilestoneMovesToInReview();
        RegionalTimesheetMilestoneRejectsMovesFromArchived();
    }
}
