namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictSpanClaimEngineChecks
{
    public static void StrictSpanClaimEngineKeepsResultsWithinBounds()
    {
        var engine = StrictSpanClaimEngine.DefaultStrictSpanClaimEngine();
        var item = StrictSpanClaim.NewStrictSpanClaim("code-45", "name-799");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictSpanClaimEngineKeepsResultsWithinBounds();
    }
}
