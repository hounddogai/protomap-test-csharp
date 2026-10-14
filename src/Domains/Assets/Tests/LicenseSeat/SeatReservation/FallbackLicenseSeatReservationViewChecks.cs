namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;

/// <summary>Checks of seat reservation code, run by the project's runner.</summary>
public static class FallbackLicenseSeatReservationViewChecks
{
    public static void FallbackLicenseSeatReservationViewShowsTheStatusInCapitals()
    {
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("code-285", "name-192");
        var view = FallbackLicenseSeatReservationView.ToFallbackLicenseSeatReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackLicenseSeatReservationView.ToFallbackLicenseSeatReservationViews(new List<FallbackLicenseSeatReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatReservationViewShowsTheStatusInCapitals();
    }
}
