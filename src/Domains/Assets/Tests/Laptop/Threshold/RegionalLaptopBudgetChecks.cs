namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RegionalLaptopBudgetChecks
{
    public static void RegionalLaptopBudgetStartsInNew()
    {
        var item = RegionalLaptopBudget.NewRegionalLaptopBudget("code-372", "name-57");
        Check.Equal(item.Status, RegionalLaptopBudgetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalLaptopBudgetMovesToQueued()
    {
        var item = RegionalLaptopBudget.NewRegionalLaptopBudget("code-372", "name-57");
        try
        {
            item.MoveTo(RegionalLaptopBudgetStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalLaptopBudgetStatus.Queued, "status");
    }

    public static void RegionalLaptopBudgetRejectsMovesFromFailed()
    {
        var item = RegionalLaptopBudget.NewRegionalLaptopBudget("code-372", "name-57");
        item.Status = RegionalLaptopBudgetStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalLaptopBudgetStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalLaptopBudgetStartsInNew();
        RegionalLaptopBudgetMovesToQueued();
        RegionalLaptopBudgetRejectsMovesFromFailed();
    }
}
