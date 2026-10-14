namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientLicenseSeatScheduleViewChecks
{
    public static void LenientLicenseSeatScheduleViewShowsTheStatusInCapitals()
    {
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("code-531", "label-125");
        var view = LenientLicenseSeatScheduleView.ToLenientLicenseSeatScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientLicenseSeatScheduleView.ToLenientLicenseSeatScheduleViews(new List<LenientLicenseSeatSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientLicenseSeatScheduleViewShowsTheStatusInCapitals();
    }
}
