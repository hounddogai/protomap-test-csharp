namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanAllocationChecks
{
    public static void ReturnPlanAllocationStartsInNew()
    {
        var item = ReturnPlanAllocation.NewReturnPlanAllocation("code-625", "name-28");
        Check.Equal(item.Status, ReturnPlanAllocationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanAllocationMovesToRunning()
    {
        var item = ReturnPlanAllocation.NewReturnPlanAllocation("code-625", "name-28");
        try
        {
            item.MoveTo(ReturnPlanAllocationStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanAllocationStatus.Running, "status");
    }

    public static void ReturnPlanAllocationRejectsMovesFromFailed()
    {
        var item = ReturnPlanAllocation.NewReturnPlanAllocation("code-625", "name-28");
        item.Status = ReturnPlanAllocationStatus.Failed;
        Check.Fails(() => item.MoveTo(ReturnPlanAllocationStatus.New), "move");
    }

    public static void RunAll()
    {
        ReturnPlanAllocationStartsInNew();
        ReturnPlanAllocationMovesToRunning();
        ReturnPlanAllocationRejectsMovesFromFailed();
    }
}
