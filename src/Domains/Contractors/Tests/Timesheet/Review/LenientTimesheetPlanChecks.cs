namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientTimesheetPlanChecks
{
    public static void LenientTimesheetPlanStartsInNew()
    {
        var item = LenientTimesheetPlan.NewLenientTimesheetPlan("code-394", "name-992");
        Check.Equal(item.Status, LenientTimesheetPlanStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientTimesheetPlanMovesToRunning()
    {
        var item = LenientTimesheetPlan.NewLenientTimesheetPlan("code-394", "name-992");
        try
        {
            item.MoveTo(LenientTimesheetPlanStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientTimesheetPlanStatus.Running, "status");
    }

    public static void LenientTimesheetPlanRejectsMovesFromFailed()
    {
        var item = LenientTimesheetPlan.NewLenientTimesheetPlan("code-394", "name-992");
        item.Status = LenientTimesheetPlanStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientTimesheetPlanStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientTimesheetPlanStartsInNew();
        LenientTimesheetPlanMovesToRunning();
        LenientTimesheetPlanRejectsMovesFromFailed();
    }
}
