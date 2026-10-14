namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class PendingLicenseSeatForecastEngineChecks
{
    public static void PendingLicenseSeatForecastEngineKeepsResultsWithinBounds()
    {
        var engine = PendingLicenseSeatForecastEngine.DefaultPendingLicenseSeatForecastEngine();
        var item = PendingLicenseSeatForecast.NewPendingLicenseSeatForecast("code-859", "title-746");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingLicenseSeatForecastEngineKeepsResultsWithinBounds();
    }
}
