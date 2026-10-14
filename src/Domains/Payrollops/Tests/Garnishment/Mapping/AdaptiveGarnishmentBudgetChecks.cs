namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentBudgetChecks
{
    public static void AdaptiveGarnishmentBudgetStartsInIdle()
    {
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("code-457", "title-736");
        Check.Equal(item.Status, AdaptiveGarnishmentBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveGarnishmentBudgetMovesToReady()
    {
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("code-457", "title-736");
        try
        {
            item.MoveTo(AdaptiveGarnishmentBudgetStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveGarnishmentBudgetStatus.Ready, "status");
    }

    public static void AdaptiveGarnishmentBudgetRejectsMovesFromStopped()
    {
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("code-457", "title-736");
        item.Status = AdaptiveGarnishmentBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveGarnishmentBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentBudgetStartsInIdle();
        AdaptiveGarnishmentBudgetMovesToReady();
        AdaptiveGarnishmentBudgetRejectsMovesFromStopped();
    }
}
