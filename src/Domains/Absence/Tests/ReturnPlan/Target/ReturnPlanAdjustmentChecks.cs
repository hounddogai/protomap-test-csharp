namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanAdjustmentChecks
{
    public static void ReturnPlanAdjustmentStartsInOpen()
    {
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("code-505", "name-901");
        Check.Equal(item.Status, ReturnPlanAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanAdjustmentMovesToResolved()
    {
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("code-505", "name-901");
        try
        {
            item.MoveTo(ReturnPlanAdjustmentStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanAdjustmentStatus.Resolved, "status");
    }

    public static void ReturnPlanAdjustmentRejectsMovesFromArchived()
    {
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("code-505", "name-901");
        item.Status = ReturnPlanAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(ReturnPlanAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        ReturnPlanAdjustmentStartsInOpen();
        ReturnPlanAdjustmentMovesToResolved();
        ReturnPlanAdjustmentRejectsMovesFromArchived();
    }
}
