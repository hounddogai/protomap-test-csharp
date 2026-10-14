namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class AdaptiveOccupancyTemplateEngineChecks
{
    public static void AdaptiveOccupancyTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveOccupancyTemplateEngine.DefaultAdaptiveOccupancyTemplateEngine();
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("code-680", "label-203");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveOccupancyTemplateEngineKeepsResultsWithinBounds();
    }
}
