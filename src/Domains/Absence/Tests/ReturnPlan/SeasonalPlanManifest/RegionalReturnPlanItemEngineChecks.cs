namespace Acme.Workforce.Absence.Tests.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

/// <summary>Checks of seasonal plan manifest code, run by the project's runner.</summary>
public static class RegionalReturnPlanItemEngineChecks
{
    public static void RegionalReturnPlanItemEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalReturnPlanItemEngine.DefaultRegionalReturnPlanItemEngine();
        var item = RegionalReturnPlanItem.NewRegionalReturnPlanItem("code-523", "name-239");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalReturnPlanItemEngineKeepsResultsWithinBounds();
    }
}
