namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class PendingLicenseSeatForecastChecks
{
    public static void PendingLicenseSeatForecastStartsInPlanned()
    {
        var item = PendingLicenseSeatForecast.NewPendingLicenseSeatForecast("code-859", "title-746");
        Check.Equal(item.Status, PendingLicenseSeatForecastStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingLicenseSeatForecastMovesToScheduled()
    {
        var item = PendingLicenseSeatForecast.NewPendingLicenseSeatForecast("code-859", "title-746");
        try
        {
            item.MoveTo(PendingLicenseSeatForecastStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingLicenseSeatForecastStatus.Scheduled, "status");
    }

    public static void PendingLicenseSeatForecastRejectsMovesFromCancelled()
    {
        var item = PendingLicenseSeatForecast.NewPendingLicenseSeatForecast("code-859", "title-746");
        item.Status = PendingLicenseSeatForecastStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingLicenseSeatForecastStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingLicenseSeatForecastStartsInPlanned();
        PendingLicenseSeatForecastMovesToScheduled();
        PendingLicenseSeatForecastRejectsMovesFromCancelled();
    }
}
