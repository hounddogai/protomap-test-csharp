namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanAdjustmentEngineChecks
{
    public static void SpanAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = SpanAdjustmentEngine.DefaultSpanAdjustmentEngine();
        var item = SpanAdjustment.NewSpanAdjustment("code-315", "name-222");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanAdjustmentEngineKeepsResultsWithinBounds();
    }
}
