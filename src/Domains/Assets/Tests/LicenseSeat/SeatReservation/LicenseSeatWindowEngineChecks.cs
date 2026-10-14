namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of seat reservation code, run by the project's runner.</summary>
public static class LicenseSeatWindowEngineChecks
{
    public static void LicenseSeatWindowEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatWindowEngine.DefaultLicenseSeatWindowEngine(Flags.NewRegistry());
        var item = LicenseSeatWindow.NewLicenseSeatWindow("code-507", "title-789");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatWindowEngineKeepsResultsWithinBounds();
    }
}
