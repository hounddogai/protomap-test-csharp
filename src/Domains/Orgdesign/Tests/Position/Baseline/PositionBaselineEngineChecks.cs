namespace Acme.Workforce.Orgdesign.Tests.Position.Baseline;

using Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PositionBaselineEngineChecks
{
    public static void PositionBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = PositionBaselineEngine.DefaultPositionBaselineEngine();
        var item = PositionBaseline.NewPositionBaseline("code-945", "label-153");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionBaselineEngineKeepsResultsWithinBounds();
    }
}
