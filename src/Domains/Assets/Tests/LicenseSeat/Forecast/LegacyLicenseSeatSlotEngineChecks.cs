namespace Acme.Workforce.Assets.Tests.LicenseSeat.Forecast;

using Acme.Workforce.Assets.LicenseSeat.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LegacyLicenseSeatSlotEngineChecks
{
    public static void LegacyLicenseSeatSlotEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyLicenseSeatSlotEngine.DefaultLegacyLicenseSeatSlotEngine();
        var item = LegacyLicenseSeatSlot.NewLegacyLicenseSeatSlot("code-713", "name-63");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyLicenseSeatSlotEngineKeepsResultsWithinBounds();
    }
}
