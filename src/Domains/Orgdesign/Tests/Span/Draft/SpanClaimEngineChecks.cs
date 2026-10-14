namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanClaimEngineChecks
{
    public static void SpanClaimEngineKeepsResultsWithinBounds()
    {
        var engine = SpanClaimEngine.DefaultSpanClaimEngine(Flags.NewRegistry());
        var item = SpanClaim.NewSpanClaim("code-490", "name-450");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanClaimEngineKeepsResultsWithinBounds();
    }
}
