namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class OccupancyProfileEngineChecks
{
    public static void OccupancyProfileEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyProfileEngine.DefaultOccupancyProfileEngine(Flags.NewRegistry());
        var item = OccupancyProfile.NewOccupancyProfile("code-289", "title-856");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyProfileEngineKeepsResultsWithinBounds();
    }
}
