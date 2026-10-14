namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class PrimaryOccupancyManifestEngineChecks
{
    public static void PrimaryOccupancyManifestEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryOccupancyManifestEngine.DefaultPrimaryOccupancyManifestEngine(Flags.NewRegistry());
        var item = PrimaryOccupancyManifest.NewPrimaryOccupancyManifest("code-196", "name-312");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryOccupancyManifestEngineKeepsResultsWithinBounds();
    }
}
