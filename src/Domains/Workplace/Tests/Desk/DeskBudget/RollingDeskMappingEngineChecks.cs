namespace Acme.Workforce.Workplace.Tests.Desk.DeskBudget;

using Acme.Workforce.Workplace.Desk.DeskBudget;

/// <summary>Checks of desk budget code, run by the project's runner.</summary>
public static class RollingDeskMappingEngineChecks
{
    public static void RollingDeskMappingEngineKeepsResultsWithinBounds()
    {
        var engine = RollingDeskMappingEngine.DefaultRollingDeskMappingEngine();
        var item = RollingDeskMapping.NewRollingDeskMapping("code-377", "label-746");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingDeskMappingEngineKeepsResultsWithinBounds();
    }
}
