namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class LicenseSeatGroupEngineChecks
{
    public static void LicenseSeatGroupEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatGroupEngine.DefaultLicenseSeatGroupEngine();
        var item = LicenseSeatGroup.NewLicenseSeatGroup("code-878", "label-817");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatGroupEngineKeepsResultsWithinBounds();
    }
}
