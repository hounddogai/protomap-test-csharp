namespace Acme.Workforce.Assets.Tests.Refresh.Review;

using Acme.Workforce.Assets.Refresh.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PrimaryRefreshVarianceEngineChecks
{
    public static void PrimaryRefreshVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryRefreshVarianceEngine.DefaultPrimaryRefreshVarianceEngine(Flags.NewRegistry());
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("code-763", "label-978");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryRefreshVarianceEngineKeepsResultsWithinBounds();
    }
}
