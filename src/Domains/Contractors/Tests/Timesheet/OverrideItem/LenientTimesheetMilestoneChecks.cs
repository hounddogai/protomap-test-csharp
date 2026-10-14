namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class LenientTimesheetMilestoneChecks
{
    public static void LenientTimesheetMilestoneStartsInNew()
    {
        var item = LenientTimesheetMilestone.NewLenientTimesheetMilestone("code-770", "label-384");
        Check.Equal(item.Status, LenientTimesheetMilestoneStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientTimesheetMilestoneMovesToQueued()
    {
        var item = LenientTimesheetMilestone.NewLenientTimesheetMilestone("code-770", "label-384");
        try
        {
            item.MoveTo(LenientTimesheetMilestoneStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientTimesheetMilestoneStatus.Queued, "status");
    }

    public static void LenientTimesheetMilestoneRejectsMovesFromFailed()
    {
        var item = LenientTimesheetMilestone.NewLenientTimesheetMilestone("code-770", "label-384");
        item.Status = LenientTimesheetMilestoneStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientTimesheetMilestoneStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientTimesheetMilestoneStartsInNew();
        LenientTimesheetMilestoneMovesToQueued();
        LenientTimesheetMilestoneRejectsMovesFromFailed();
    }
}
