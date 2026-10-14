namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientGarnishmentBudgetChecks
{
    public static void LenientGarnishmentBudgetStartsInIdle()
    {
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("code-250", "label-403");
        Check.Equal(item.Status, LenientGarnishmentBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientGarnishmentBudgetMovesToReady()
    {
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("code-250", "label-403");
        try
        {
            item.MoveTo(LenientGarnishmentBudgetStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientGarnishmentBudgetStatus.Ready, "status");
    }

    public static void LenientGarnishmentBudgetRejectsMovesFromStopped()
    {
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("code-250", "label-403");
        item.Status = LenientGarnishmentBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientGarnishmentBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientGarnishmentBudgetStartsInIdle();
        LenientGarnishmentBudgetMovesToReady();
        LenientGarnishmentBudgetRejectsMovesFromStopped();
    }
}
