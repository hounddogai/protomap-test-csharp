namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class EquityGrantMappingEngineChecks
{
    public static void EquityGrantMappingEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantMappingEngine.DefaultEquityGrantMappingEngine(Flags.NewRegistry());
        var item = EquityGrantMapping.NewEquityGrantMapping("code-746", "name-270");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantMappingEngineKeepsResultsWithinBounds();
    }
}
