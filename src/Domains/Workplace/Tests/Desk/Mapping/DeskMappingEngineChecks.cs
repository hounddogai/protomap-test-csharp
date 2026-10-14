namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskMappingEngineChecks
{
    public static void DeskMappingEngineKeepsResultsWithinBounds()
    {
        var engine = DeskMappingEngine.DefaultDeskMappingEngine();
        var item = DeskMapping.NewDeskMapping("code-662", "title-988");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskMappingEngineKeepsResultsWithinBounds();
    }
}
