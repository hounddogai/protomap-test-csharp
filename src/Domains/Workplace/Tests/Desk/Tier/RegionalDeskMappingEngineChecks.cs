namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalDeskMappingEngineChecks
{
    public static void RegionalDeskMappingEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalDeskMappingEngine.DefaultRegionalDeskMappingEngine(Flags.NewRegistry());
        var item = RegionalDeskMapping.NewRegionalDeskMapping("code-235", "title-958");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalDeskMappingEngineKeepsResultsWithinBounds();
    }
}
