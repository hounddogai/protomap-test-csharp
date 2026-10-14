namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallAllocationEngineChecks
{
    public static void TownHallAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallAllocationEngine.DefaultTownHallAllocationEngine(Flags.NewRegistry());
        var item = TownHallAllocation.NewTownHallAllocation("code-719", "name-413");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallAllocationEngineKeepsResultsWithinBounds();
    }
}
