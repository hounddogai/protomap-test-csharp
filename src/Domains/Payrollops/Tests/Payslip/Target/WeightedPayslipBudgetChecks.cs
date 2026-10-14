namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class WeightedPayslipBudgetChecks
{
    public static void WeightedPayslipBudgetStartsInIdle()
    {
        var item = WeightedPayslipBudget.NewWeightedPayslipBudget("code-645", "title-898");
        Check.Equal(item.Status, WeightedPayslipBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedPayslipBudgetMovesToReady()
    {
        var item = WeightedPayslipBudget.NewWeightedPayslipBudget("code-645", "title-898");
        try
        {
            item.MoveTo(WeightedPayslipBudgetStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedPayslipBudgetStatus.Ready, "status");
    }

    public static void WeightedPayslipBudgetRejectsMovesFromStopped()
    {
        var item = WeightedPayslipBudget.NewWeightedPayslipBudget("code-645", "title-898");
        item.Status = WeightedPayslipBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(WeightedPayslipBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WeightedPayslipBudgetStartsInIdle();
        WeightedPayslipBudgetMovesToReady();
        WeightedPayslipBudgetRejectsMovesFromStopped();
    }
}
