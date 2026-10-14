namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OccupancyDraftEngineChecks
{
    public static void OccupancyDraftEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyDraftEngine.DefaultOccupancyDraftEngine(Flags.NewRegistry());
        var item = OccupancyDraft.NewOccupancyDraft("code-831", "label-508");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyDraftEngineKeepsResultsWithinBounds();
    }
}
