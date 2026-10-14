namespace Acme.Workforce.Workplace.Tests.Desk.Snapshot;

using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RegionalDeskCycleEngineChecks
{
    public static void RegionalDeskCycleEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalDeskCycleEngine.DefaultRegionalDeskCycleEngine();
        var item = RegionalDeskCycle.NewRegionalDeskCycle("code-763", "name-503");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalDeskCycleEngineKeepsResultsWithinBounds();
    }
}
