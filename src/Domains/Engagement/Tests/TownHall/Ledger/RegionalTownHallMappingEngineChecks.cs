namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalTownHallMappingEngineChecks
{
    public static void RegionalTownHallMappingEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalTownHallMappingEngine.DefaultRegionalTownHallMappingEngine();
        var item = RegionalTownHallMapping.NewRegionalTownHallMapping("code-251", "name-825");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalTownHallMappingEngineKeepsResultsWithinBounds();
    }
}
