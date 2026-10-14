namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatMappingEngineChecks
{
    public static void LicenseSeatMappingEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatMappingEngine.DefaultLicenseSeatMappingEngine(Flags.NewRegistry());
        var item = LicenseSeatMapping.NewLicenseSeatMapping("code-479", "title-722");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatMappingEngineKeepsResultsWithinBounds();
    }
}
