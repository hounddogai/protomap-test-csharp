namespace Acme.Workforce.Workplace.Tests.Occupancy.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class OccupancyMilestoneEngineChecks
{
    public static void OccupancyMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyMilestoneEngine.DefaultOccupancyMilestoneEngine(Flags.NewRegistry());
        var item = OccupancyMilestone.NewOccupancyMilestone("code-397", "name-891");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyMilestoneEngineKeepsResultsWithinBounds();
    }
}
