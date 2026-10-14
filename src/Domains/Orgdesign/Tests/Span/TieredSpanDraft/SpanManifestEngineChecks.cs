namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanManifestEngineChecks
{
    public static void SpanManifestEngineKeepsResultsWithinBounds()
    {
        var engine = SpanManifestEngine.DefaultSpanManifestEngine();
        var item = SpanManifest.NewSpanManifest("code-151", "title-379");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanManifestEngineKeepsResultsWithinBounds();
    }
}
