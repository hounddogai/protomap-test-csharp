namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingRefreshClaimEngineChecks
{
    public static void PendingRefreshClaimEngineKeepsResultsWithinBounds()
    {
        var engine = PendingRefreshClaimEngine.DefaultPendingRefreshClaimEngine(Flags.NewRegistry());
        var item = PendingRefreshClaim.NewPendingRefreshClaim("code-247", "title-488");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingRefreshClaimEngineKeepsResultsWithinBounds();
    }
}
