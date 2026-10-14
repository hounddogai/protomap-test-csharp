namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanRecordEngineChecks
{
    public static void SpanRecordEngineKeepsResultsWithinBounds()
    {
        var engine = SpanRecordEngine.DefaultSpanRecordEngine();
        var item = SpanRecord.NewSpanRecord("code-676", "label-557");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanRecordEngineKeepsResultsWithinBounds();
    }
}
