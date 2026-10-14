namespace Acme.Workforce.Orgdesign.Tests.Span.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SpanDigestEngineChecks
{
    public static void SpanDigestEngineKeepsResultsWithinBounds()
    {
        var engine = SpanDigestEngine.DefaultSpanDigestEngine(Flags.NewRegistry());
        var item = SpanDigest.NewSpanDigest("code-675", "name-571");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanDigestEngineKeepsResultsWithinBounds();
    }
}
