namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class WeightedGarnishmentBudgetChecks
{
    public static void WeightedGarnishmentBudgetStartsInRequested()
    {
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("code-706", "name-711");
        Check.Equal(item.Status, WeightedGarnishmentBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedGarnishmentBudgetMovesToGranted()
    {
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("code-706", "name-711");
        try
        {
            item.MoveTo(WeightedGarnishmentBudgetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedGarnishmentBudgetStatus.Granted, "status");
    }

    public static void WeightedGarnishmentBudgetRejectsMovesFromExpired()
    {
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("code-706", "name-711");
        item.Status = WeightedGarnishmentBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedGarnishmentBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedGarnishmentBudgetStartsInRequested();
        WeightedGarnishmentBudgetMovesToGranted();
        WeightedGarnishmentBudgetRejectsMovesFromExpired();
    }
}
