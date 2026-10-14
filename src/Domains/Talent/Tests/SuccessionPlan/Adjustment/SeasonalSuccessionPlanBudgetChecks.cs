namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanBudgetChecks
{
    public static void SeasonalSuccessionPlanBudgetStartsInRequested()
    {
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("code-355", "label-953");
        Check.Equal(item.Status, SeasonalSuccessionPlanBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalSuccessionPlanBudgetMovesToGranted()
    {
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("code-355", "label-953");
        try
        {
            item.MoveTo(SeasonalSuccessionPlanBudgetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalSuccessionPlanBudgetStatus.Granted, "status");
    }

    public static void SeasonalSuccessionPlanBudgetRejectsMovesFromExpired()
    {
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("code-355", "label-953");
        item.Status = SeasonalSuccessionPlanBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalSuccessionPlanBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanBudgetStartsInRequested();
        SeasonalSuccessionPlanBudgetMovesToGranted();
        SeasonalSuccessionPlanBudgetRejectsMovesFromExpired();
    }
}
