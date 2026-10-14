namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class WarrantyBudgetChecks
{
    public static void WarrantyBudgetStartsInNew()
    {
        var item = WarrantyBudget.NewWarrantyBudget("code-394", "name-262");
        Check.Equal(item.Status, WarrantyBudgetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyBudgetMovesToQueued()
    {
        var item = WarrantyBudget.NewWarrantyBudget("code-394", "name-262");
        try
        {
            item.MoveTo(WarrantyBudgetStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyBudgetStatus.Queued, "status");
    }

    public static void WarrantyBudgetRejectsMovesFromFailed()
    {
        var item = WarrantyBudget.NewWarrantyBudget("code-394", "name-262");
        item.Status = WarrantyBudgetStatus.Failed;
        Check.Fails(() => item.MoveTo(WarrantyBudgetStatus.New), "move");
    }

    public static void RunAll()
    {
        WarrantyBudgetStartsInNew();
        WarrantyBudgetMovesToQueued();
        WarrantyBudgetRejectsMovesFromFailed();
    }
}
