namespace Acme.Workforce.Contractors.Tests.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FallbackTimesheetPlanChecks
{
    public static void FallbackTimesheetPlanStartsInOpen()
    {
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("code-925", "name-646");
        Check.Equal(item.Status, FallbackTimesheetPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackTimesheetPlanMovesToInReview()
    {
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("code-925", "name-646");
        try
        {
            item.MoveTo(FallbackTimesheetPlanStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackTimesheetPlanStatus.InReview, "status");
    }

    public static void FallbackTimesheetPlanRejectsMovesFromArchived()
    {
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("code-925", "name-646");
        item.Status = FallbackTimesheetPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackTimesheetPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackTimesheetPlanStartsInOpen();
        FallbackTimesheetPlanMovesToInReview();
        FallbackTimesheetPlanRejectsMovesFromArchived();
    }
}
