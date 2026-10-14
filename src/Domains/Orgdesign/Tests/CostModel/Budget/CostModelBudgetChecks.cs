namespace Acme.Workforce.Orgdesign.Tests.CostModel.Budget;

using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CostModelBudgetChecks
{
    public static void CostModelBudgetStartsInIdle()
    {
        var item = CostModelBudget.NewCostModelBudget("code-186", "title-135");
        Check.Equal(item.Status, CostModelBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelBudgetMovesToStopped()
    {
        var item = CostModelBudget.NewCostModelBudget("code-186", "title-135");
        try
        {
            item.MoveTo(CostModelBudgetStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelBudgetStatus.Stopped, "status");
    }

    public static void CostModelBudgetRejectsMovesFromStopped()
    {
        var item = CostModelBudget.NewCostModelBudget("code-186", "title-135");
        item.Status = CostModelBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(CostModelBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CostModelBudgetStartsInIdle();
        CostModelBudgetMovesToStopped();
        CostModelBudgetRejectsMovesFromStopped();
    }
}
