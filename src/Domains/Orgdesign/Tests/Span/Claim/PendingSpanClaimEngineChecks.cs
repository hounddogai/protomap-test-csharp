namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PendingSpanClaimEngineChecks
{
    public static void PendingSpanClaimEngineKeepsResultsWithinBounds()
    {
        var engine = PendingSpanClaimEngine.DefaultPendingSpanClaimEngine(Flags.NewRegistry());
        var item = PendingSpanClaim.NewPendingSpanClaim("code-387", "label-432");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingSpanClaimEngineKeepsResultsWithinBounds();
    }
}
