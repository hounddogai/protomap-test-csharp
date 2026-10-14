namespace Acme.Workforce.Payrollops.Tests.Garnishment.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GarnishmentMappingEngineChecks
{
    public static void GarnishmentMappingEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentMappingEngine.DefaultGarnishmentMappingEngine(Flags.NewRegistry());
        var item = GarnishmentMapping.NewGarnishmentMapping("code-408", "name-415");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentMappingEngineKeepsResultsWithinBounds();
    }
}
