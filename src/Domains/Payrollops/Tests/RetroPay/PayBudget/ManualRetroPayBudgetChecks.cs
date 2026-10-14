namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayBudget;

using Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>Checks of pay budget code, run by the project's runner.</summary>
public static class ManualRetroPayBudgetChecks
{
    public static void ManualRetroPayBudgetStartsInIdle()
    {
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("code-408", "label-66");
        Check.Equal(item.Status, ManualRetroPayBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualRetroPayBudgetMovesToStopped()
    {
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("code-408", "label-66");
        try
        {
            item.MoveTo(ManualRetroPayBudgetStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualRetroPayBudgetStatus.Stopped, "status");
    }

    public static void ManualRetroPayBudgetRejectsMovesFromStopped()
    {
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("code-408", "label-66");
        item.Status = ManualRetroPayBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualRetroPayBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualRetroPayBudgetStartsInIdle();
        ManualRetroPayBudgetMovesToStopped();
        ManualRetroPayBudgetRejectsMovesFromStopped();
    }
}
