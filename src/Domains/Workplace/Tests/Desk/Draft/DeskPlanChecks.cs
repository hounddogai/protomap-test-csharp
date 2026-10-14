namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class DeskPlanChecks
{
    public static void DeskPlanStartsInRequested()
    {
        var item = DeskPlan.NewDeskPlan("code-78", "title-351");
        Check.Equal(item.Status, DeskPlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskPlanMovesToGranted()
    {
        var item = DeskPlan.NewDeskPlan("code-78", "title-351");
        try
        {
            item.MoveTo(DeskPlanStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskPlanStatus.Granted, "status");
    }

    public static void DeskPlanRejectsMovesFromExpired()
    {
        var item = DeskPlan.NewDeskPlan("code-78", "title-351");
        item.Status = DeskPlanStatus.Expired;
        Check.Fails(() => item.MoveTo(DeskPlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DeskPlanStartsInRequested();
        DeskPlanMovesToGranted();
        DeskPlanRejectsMovesFromExpired();
    }
}
