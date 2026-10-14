namespace Acme.Workforce.Payrollops.Tests.PayRun.Budget;

using Acme.Workforce.Payrollops.PayRun.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class ScheduledPayRunBudgetChecks
{
    public static void ScheduledPayRunBudgetStartsInNew()
    {
        var item = ScheduledPayRunBudget.NewScheduledPayRunBudget("code-223", "name-630");
        Check.Equal(item.Status, ScheduledPayRunBudgetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayRunBudgetMovesToQueued()
    {
        var item = ScheduledPayRunBudget.NewScheduledPayRunBudget("code-223", "name-630");
        try
        {
            item.MoveTo(ScheduledPayRunBudgetStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayRunBudgetStatus.Queued, "status");
    }

    public static void ScheduledPayRunBudgetRejectsMovesFromFailed()
    {
        var item = ScheduledPayRunBudget.NewScheduledPayRunBudget("code-223", "name-630");
        item.Status = ScheduledPayRunBudgetStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledPayRunBudgetStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledPayRunBudgetStartsInNew();
        ScheduledPayRunBudgetMovesToQueued();
        ScheduledPayRunBudgetRejectsMovesFromFailed();
    }
}
