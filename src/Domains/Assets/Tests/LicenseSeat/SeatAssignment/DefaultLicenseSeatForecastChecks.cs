namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class DefaultLicenseSeatForecastChecks
{
    public static void DefaultLicenseSeatForecastStartsInCreated()
    {
        var item = DefaultLicenseSeatForecast.NewDefaultLicenseSeatForecast("code-454", "label-887");
        Check.Equal(item.Status, DefaultLicenseSeatForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultLicenseSeatForecastMovesToValidated()
    {
        var item = DefaultLicenseSeatForecast.NewDefaultLicenseSeatForecast("code-454", "label-887");
        try
        {
            item.MoveTo(DefaultLicenseSeatForecastStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultLicenseSeatForecastStatus.Validated, "status");
    }

    public static void DefaultLicenseSeatForecastRejectsMovesFromRetired()
    {
        var item = DefaultLicenseSeatForecast.NewDefaultLicenseSeatForecast("code-454", "label-887");
        item.Status = DefaultLicenseSeatForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultLicenseSeatForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatForecastStartsInCreated();
        DefaultLicenseSeatForecastMovesToValidated();
        DefaultLicenseSeatForecastRejectsMovesFromRetired();
    }
}
