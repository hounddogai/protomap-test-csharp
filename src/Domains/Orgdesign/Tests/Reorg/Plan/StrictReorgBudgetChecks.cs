namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class StrictReorgBudgetChecks
{
    public static void StrictReorgBudgetStartsInCreated()
    {
        var item = StrictReorgBudget.NewStrictReorgBudget("code-640", "name-345");
        Check.Equal(item.Status, StrictReorgBudgetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictReorgBudgetMovesToPublished()
    {
        var item = StrictReorgBudget.NewStrictReorgBudget("code-640", "name-345");
        try
        {
            item.MoveTo(StrictReorgBudgetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictReorgBudgetStatus.Published, "status");
    }

    public static void StrictReorgBudgetRejectsMovesFromRetired()
    {
        var item = StrictReorgBudget.NewStrictReorgBudget("code-640", "name-345");
        item.Status = StrictReorgBudgetStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictReorgBudgetStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictReorgBudgetStartsInCreated();
        StrictReorgBudgetMovesToPublished();
        StrictReorgBudgetRejectsMovesFromRetired();
    }
}
