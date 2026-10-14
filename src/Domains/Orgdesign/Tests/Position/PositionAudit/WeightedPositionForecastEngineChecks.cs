namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>Checks of position audit code, run by the project's runner.</summary>
public static class WeightedPositionForecastEngineChecks
{
    public static void WeightedPositionForecastEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedPositionForecastEngine.DefaultWeightedPositionForecastEngine(Flags.NewRegistry());
        var item = WeightedPositionForecast.NewWeightedPositionForecast("code-219", "label-757");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedPositionForecastEngineKeepsResultsWithinBounds();
    }
}
