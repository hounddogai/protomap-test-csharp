namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SpanGroupEngineChecks
{
    public static void SpanGroupEngineKeepsResultsWithinBounds()
    {
        var engine = SpanGroupEngine.DefaultSpanGroupEngine(Flags.NewRegistry());
        var item = SpanGroup.NewSpanGroup("code-261", "label-737");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanGroupEngineKeepsResultsWithinBounds();
    }
}
