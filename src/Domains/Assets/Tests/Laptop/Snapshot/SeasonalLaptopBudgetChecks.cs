namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalLaptopBudgetChecks
{
    public static void SeasonalLaptopBudgetStartsInNew()
    {
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("code-887", "name-748");
        Check.Equal(item.Status, SeasonalLaptopBudgetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalLaptopBudgetMovesToQueued()
    {
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("code-887", "name-748");
        try
        {
            item.MoveTo(SeasonalLaptopBudgetStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalLaptopBudgetStatus.Queued, "status");
    }

    public static void SeasonalLaptopBudgetRejectsMovesFromFailed()
    {
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("code-887", "name-748");
        item.Status = SeasonalLaptopBudgetStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalLaptopBudgetStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalLaptopBudgetStartsInNew();
        SeasonalLaptopBudgetMovesToQueued();
        SeasonalLaptopBudgetRejectsMovesFromFailed();
    }
}
