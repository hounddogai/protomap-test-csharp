namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalSpanDigestEngineChecks
{
    public static void SeasonalSpanDigestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalSpanDigestEngine.DefaultSeasonalSpanDigestEngine(Flags.NewRegistry());
        var item = SeasonalSpanDigest.NewSeasonalSpanDigest("code-5", "name-717");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalSpanDigestEngineKeepsResultsWithinBounds();
    }
}
