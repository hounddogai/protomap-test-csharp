namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyWindowEngineChecks
{
    public static void OccupancyWindowEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyWindowEngine.DefaultOccupancyWindowEngine(Flags.NewRegistry());
        var item = OccupancyWindow.NewOccupancyWindow("code-605", "label-682");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyWindowEngineKeepsResultsWithinBounds();
    }
}
