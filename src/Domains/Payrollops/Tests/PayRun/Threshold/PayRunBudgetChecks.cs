namespace Acme.Workforce.Payrollops.Tests.PayRun.Threshold;

using Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayRunBudgetChecks
{
    public static void PayRunBudgetStartsInDraft()
    {
        var item = PayRunBudget.NewPayRunBudget("code-742", "name-701");
        Check.Equal(item.Status, PayRunBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunBudgetMovesToActive()
    {
        var item = PayRunBudget.NewPayRunBudget("code-742", "name-701");
        try
        {
            item.MoveTo(PayRunBudgetStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunBudgetStatus.Active, "status");
    }

    public static void PayRunBudgetRejectsMovesFromClosed()
    {
        var item = PayRunBudget.NewPayRunBudget("code-742", "name-701");
        item.Status = PayRunBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(PayRunBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PayRunBudgetStartsInDraft();
        PayRunBudgetMovesToActive();
        PayRunBudgetRejectsMovesFromClosed();
    }
}
