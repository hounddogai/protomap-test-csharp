namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class GlobalLeaveRequestBudgetChecks
{
    public static void GlobalLeaveRequestBudgetStartsInDraft()
    {
        var item = GlobalLeaveRequestBudget.NewGlobalLeaveRequestBudget("code-907", "name-469");
        Check.Equal(item.Status, GlobalLeaveRequestBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalLeaveRequestBudgetMovesToActive()
    {
        var item = GlobalLeaveRequestBudget.NewGlobalLeaveRequestBudget("code-907", "name-469");
        try
        {
            item.MoveTo(GlobalLeaveRequestBudgetStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalLeaveRequestBudgetStatus.Active, "status");
    }

    public static void GlobalLeaveRequestBudgetRejectsMovesFromClosed()
    {
        var item = GlobalLeaveRequestBudget.NewGlobalLeaveRequestBudget("code-907", "name-469");
        item.Status = GlobalLeaveRequestBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalLeaveRequestBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalLeaveRequestBudgetStartsInDraft();
        GlobalLeaveRequestBudgetMovesToActive();
        GlobalLeaveRequestBudgetRejectsMovesFromClosed();
    }
}
