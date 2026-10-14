namespace Acme.Workforce.Payrollops.Tests.Deduction.Budget;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeductionBudgetChecks
{
    public static void DeductionBudgetStartsInRequested()
    {
        var item = DeductionBudget.NewDeductionBudget("code-598", "title-974");
        Check.Equal(item.Status, DeductionBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionBudgetMovesToExpired()
    {
        var item = DeductionBudget.NewDeductionBudget("code-598", "title-974");
        try
        {
            item.MoveTo(DeductionBudgetStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionBudgetStatus.Expired, "status");
    }

    public static void DeductionBudgetRejectsMovesFromExpired()
    {
        var item = DeductionBudget.NewDeductionBudget("code-598", "title-974");
        item.Status = DeductionBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(DeductionBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DeductionBudgetStartsInRequested();
        DeductionBudgetMovesToExpired();
        DeductionBudgetRejectsMovesFromExpired();
    }
}
