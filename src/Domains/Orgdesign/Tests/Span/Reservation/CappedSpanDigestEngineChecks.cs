namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedSpanDigestEngineChecks
{
    public static void CappedSpanDigestEngineKeepsResultsWithinBounds()
    {
        var engine = CappedSpanDigestEngine.DefaultCappedSpanDigestEngine(Flags.NewRegistry());
        var item = CappedSpanDigest.NewCappedSpanDigest("code-649", "title-4");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedSpanDigestEngineKeepsResultsWithinBounds();
    }
}
