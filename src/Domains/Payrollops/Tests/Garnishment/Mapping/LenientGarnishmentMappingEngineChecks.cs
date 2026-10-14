namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LenientGarnishmentMappingEngineChecks
{
    public static void LenientGarnishmentMappingEngineKeepsResultsWithinBounds()
    {
        var engine = LenientGarnishmentMappingEngine.DefaultLenientGarnishmentMappingEngine(Flags.NewRegistry());
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("code-831", "label-388");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientGarnishmentMappingEngineKeepsResultsWithinBounds();
    }
}
