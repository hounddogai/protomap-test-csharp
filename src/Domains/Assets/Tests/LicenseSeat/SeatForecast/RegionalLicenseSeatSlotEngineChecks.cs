namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class RegionalLicenseSeatSlotEngineChecks
{
    public static void RegionalLicenseSeatSlotEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalLicenseSeatSlotEngine.DefaultRegionalLicenseSeatSlotEngine();
        var item = RegionalLicenseSeatSlot.NewRegionalLicenseSeatSlot("code-131", "name-49");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalLicenseSeatSlotEngineKeepsResultsWithinBounds();
    }
}
