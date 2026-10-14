namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanBaselineEngineChecks
{
    public static void SpanBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = SpanBaselineEngine.DefaultSpanBaselineEngine();
        var item = SpanBaseline.NewSpanBaseline("code-461", "name-42");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanBaselineEngineKeepsResultsWithinBounds();
    }
}
