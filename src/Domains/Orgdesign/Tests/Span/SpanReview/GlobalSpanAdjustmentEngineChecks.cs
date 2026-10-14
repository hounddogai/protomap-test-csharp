namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class GlobalSpanAdjustmentEngineChecks
{
    public static void GlobalSpanAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalSpanAdjustmentEngine.DefaultGlobalSpanAdjustmentEngine();
        var item = GlobalSpanAdjustment.NewGlobalSpanAdjustment("code-709", "title-59");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalSpanAdjustmentEngineKeepsResultsWithinBounds();
    }
}
