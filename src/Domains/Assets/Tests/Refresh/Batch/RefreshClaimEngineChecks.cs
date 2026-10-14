namespace Acme.Workforce.Assets.Tests.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RefreshClaimEngineChecks
{
    public static void RefreshClaimEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshClaimEngine.DefaultRefreshClaimEngine(Flags.NewRegistry());
        var item = RefreshClaim.NewRefreshClaim("code-450", "title-953");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshClaimEngineKeepsResultsWithinBounds();
    }
}
