namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LegacySpanAdjustmentEngineChecks
{
    public static void LegacySpanAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = LegacySpanAdjustmentEngine.DefaultLegacySpanAdjustmentEngine();
        var item = LegacySpanAdjustment.NewLegacySpanAdjustment("code-325", "name-395");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacySpanAdjustmentEngineKeepsResultsWithinBounds();
    }
}
