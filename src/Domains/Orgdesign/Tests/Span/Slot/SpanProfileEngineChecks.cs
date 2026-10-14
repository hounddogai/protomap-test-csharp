namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanProfileEngineChecks
{
    public static void SpanProfileEngineKeepsResultsWithinBounds()
    {
        var engine = SpanProfileEngine.DefaultSpanProfileEngine();
        var item = SpanProfile.NewSpanProfile("code-753", "label-297");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanProfileEngineKeepsResultsWithinBounds();
    }
}
