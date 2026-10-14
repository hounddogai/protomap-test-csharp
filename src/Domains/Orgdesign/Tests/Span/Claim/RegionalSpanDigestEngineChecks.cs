namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSpanDigestEngineChecks
{
    public static void RegionalSpanDigestEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalSpanDigestEngine.DefaultRegionalSpanDigestEngine();
        var item = RegionalSpanDigest.NewRegionalSpanDigest("code-45", "label-478");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalSpanDigestEngineKeepsResultsWithinBounds();
    }
}
