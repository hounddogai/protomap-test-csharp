namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class DefaultLicenseSeatMappingEngineChecks
{
    public static void DefaultLicenseSeatMappingEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultLicenseSeatMappingEngine.DefaultDefaultLicenseSeatMappingEngine(Flags.NewRegistry());
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("code-911", "title-494");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatMappingEngineKeepsResultsWithinBounds();
    }
}
