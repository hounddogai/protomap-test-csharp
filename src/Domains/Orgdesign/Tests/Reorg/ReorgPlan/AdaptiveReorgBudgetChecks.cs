namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class AdaptiveReorgBudgetChecks
{
    public static void AdaptiveReorgBudgetStartsInProposed()
    {
        var item = AdaptiveReorgBudget.NewAdaptiveReorgBudget("code-881", "title-898");
        Check.Equal(item.Status, AdaptiveReorgBudgetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveReorgBudgetMovesToApproved()
    {
        var item = AdaptiveReorgBudget.NewAdaptiveReorgBudget("code-881", "title-898");
        try
        {
            item.MoveTo(AdaptiveReorgBudgetStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveReorgBudgetStatus.Approved, "status");
    }

    public static void AdaptiveReorgBudgetRejectsMovesFromWithdrawn()
    {
        var item = AdaptiveReorgBudget.NewAdaptiveReorgBudget("code-881", "title-898");
        item.Status = AdaptiveReorgBudgetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptiveReorgBudgetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptiveReorgBudgetStartsInProposed();
        AdaptiveReorgBudgetMovesToApproved();
        AdaptiveReorgBudgetRejectsMovesFromWithdrawn();
    }
}
