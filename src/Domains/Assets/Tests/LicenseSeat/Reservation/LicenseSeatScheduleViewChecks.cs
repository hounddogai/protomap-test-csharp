namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatScheduleViewChecks
{
    public static void LicenseSeatScheduleViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("code-723", "name-412");
        var view = LicenseSeatScheduleView.ToLicenseSeatScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatScheduleView.ToLicenseSeatScheduleViews(new List<LicenseSeatSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatScheduleViewShowsTheStatusInCapitals();
    }
}
