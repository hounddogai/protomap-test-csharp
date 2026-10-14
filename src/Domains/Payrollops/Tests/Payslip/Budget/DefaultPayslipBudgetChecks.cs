namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DefaultPayslipBudgetChecks
{
    public static void DefaultPayslipBudgetStartsInOpen()
    {
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("code-673", "title-470");
        Check.Equal(item.Status, DefaultPayslipBudgetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPayslipBudgetMovesToReopened()
    {
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("code-673", "title-470");
        try
        {
            item.MoveTo(DefaultPayslipBudgetStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPayslipBudgetStatus.Reopened, "status");
    }

    public static void DefaultPayslipBudgetRejectsMovesFromArchived()
    {
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("code-673", "title-470");
        item.Status = DefaultPayslipBudgetStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultPayslipBudgetStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultPayslipBudgetStartsInOpen();
        DefaultPayslipBudgetMovesToReopened();
        DefaultPayslipBudgetRejectsMovesFromArchived();
    }
}
