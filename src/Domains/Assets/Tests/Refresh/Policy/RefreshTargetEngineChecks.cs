namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RefreshTargetEngineChecks
{
    public static void RefreshTargetEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshTargetEngine.DefaultRefreshTargetEngine(Flags.NewRegistry());
        var item = RefreshTarget.NewRefreshTarget("code-240", "title-36");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshTargetEngineKeepsResultsWithinBounds();
    }
}
