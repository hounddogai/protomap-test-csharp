namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryGarnishmentMappingEngineChecks
{
    public static void PrimaryGarnishmentMappingEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryGarnishmentMappingEngine.DefaultPrimaryGarnishmentMappingEngine();
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("code-519", "name-206");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryGarnishmentMappingEngineKeepsResultsWithinBounds();
    }
}
