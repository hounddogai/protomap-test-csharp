namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class StrictWarrantyBudgetChecks
{
    public static void StrictWarrantyBudgetStartsInNew()
    {
        var item = StrictWarrantyBudget.NewStrictWarrantyBudget("code-182", "title-64");
        Check.Equal(item.Status, StrictWarrantyBudgetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictWarrantyBudgetMovesToFailed()
    {
        var item = StrictWarrantyBudget.NewStrictWarrantyBudget("code-182", "title-64");
        try
        {
            item.MoveTo(StrictWarrantyBudgetStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictWarrantyBudgetStatus.Failed, "status");
    }

    public static void StrictWarrantyBudgetRejectsMovesFromFailed()
    {
        var item = StrictWarrantyBudget.NewStrictWarrantyBudget("code-182", "title-64");
        item.Status = StrictWarrantyBudgetStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictWarrantyBudgetStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictWarrantyBudgetStartsInNew();
        StrictWarrantyBudgetMovesToFailed();
        StrictWarrantyBudgetRejectsMovesFromFailed();
    }
}
