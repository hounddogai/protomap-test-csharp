namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshEstimateEngineChecks
{
    public static void PrimaryRefreshEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryRefreshEstimateEngine.DefaultPrimaryRefreshEstimateEngine(Flags.NewRegistry());
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("code-525", "label-623");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryRefreshEstimateEngineKeepsResultsWithinBounds();
    }
}
