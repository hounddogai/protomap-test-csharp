namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanBudgetEngineChecks
{
    public static void SeasonalSuccessionPlanBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalSuccessionPlanBudgetEngine.DefaultSeasonalSuccessionPlanBudgetEngine();
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("code-355", "label-953");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanBudgetEngineKeepsResultsWithinBounds();
    }
}
