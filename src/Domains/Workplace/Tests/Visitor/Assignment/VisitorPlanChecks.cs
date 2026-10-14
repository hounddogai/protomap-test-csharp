namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorPlanChecks
{
    public static void VisitorPlanStartsInDraft()
    {
        var item = VisitorPlan.NewVisitorPlan("code-480", "label-840");
        Check.Equal(item.Status, VisitorPlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorPlanMovesToClosed()
    {
        var item = VisitorPlan.NewVisitorPlan("code-480", "label-840");
        try
        {
            item.MoveTo(VisitorPlanStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorPlanStatus.Closed, "status");
    }

    public static void VisitorPlanRejectsMovesFromClosed()
    {
        var item = VisitorPlan.NewVisitorPlan("code-480", "label-840");
        item.Status = VisitorPlanStatus.Closed;
        Check.Fails(() => item.MoveTo(VisitorPlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        VisitorPlanStartsInDraft();
        VisitorPlanMovesToClosed();
        VisitorPlanRejectsMovesFromClosed();
    }
}
