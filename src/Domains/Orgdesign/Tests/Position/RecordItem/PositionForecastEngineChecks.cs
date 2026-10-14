namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionForecastEngineChecks
{
    public static void PositionForecastEngineKeepsResultsWithinBounds()
    {
        var engine = PositionForecastEngine.DefaultPositionForecastEngine();
        var item = PositionForecast.NewPositionForecast("code-415", "label-75");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionForecastEngineKeepsResultsWithinBounds();
    }
}
