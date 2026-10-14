namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanTargetChecks
{
    public static void ReturnPlanTargetStartsInProposed()
    {
        var item = ReturnPlanTarget.NewReturnPlanTarget("code-723", "label-310");
        Check.Equal(item.Status, ReturnPlanTargetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanTargetMovesToWithdrawn()
    {
        var item = ReturnPlanTarget.NewReturnPlanTarget("code-723", "label-310");
        try
        {
            item.MoveTo(ReturnPlanTargetStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanTargetStatus.Withdrawn, "status");
    }

    public static void ReturnPlanTargetRejectsMovesFromWithdrawn()
    {
        var item = ReturnPlanTarget.NewReturnPlanTarget("code-723", "label-310");
        item.Status = ReturnPlanTargetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ReturnPlanTargetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ReturnPlanTargetStartsInProposed();
        ReturnPlanTargetMovesToWithdrawn();
        ReturnPlanTargetRejectsMovesFromWithdrawn();
    }
}
