namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatScheduleEngineChecks
{
    public static void LicenseSeatScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatScheduleEngine.DefaultLicenseSeatScheduleEngine();
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("code-723", "name-412");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatScheduleEngineKeepsResultsWithinBounds();
    }
}
