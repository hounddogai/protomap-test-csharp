namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class SpanMappingEngineChecks
{
    public static void SpanMappingEngineKeepsResultsWithinBounds()
    {
        var engine = SpanMappingEngine.DefaultSpanMappingEngine();
        var item = SpanMapping.NewSpanMapping("code-68", "label-44");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanMappingEngineKeepsResultsWithinBounds();
    }
}
