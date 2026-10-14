namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class TieredLicenseSeatReservationEngineChecks
{
    public static void TieredLicenseSeatReservationEngineKeepsResultsWithinBounds()
    {
        var engine = TieredLicenseSeatReservationEngine.DefaultTieredLicenseSeatReservationEngine(Flags.NewRegistry());
        var item = TieredLicenseSeatReservation.NewTieredLicenseSeatReservation("code-792", "label-893");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredLicenseSeatReservationEngineKeepsResultsWithinBounds();
    }
}
