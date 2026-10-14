namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class LicenseSeatGroupViewChecks
{
    public static void LicenseSeatGroupViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatGroup.NewLicenseSeatGroup("code-878", "label-817");
        var view = LicenseSeatGroupView.ToLicenseSeatGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatGroupView.ToLicenseSeatGroupViews(new List<LicenseSeatGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatGroupViewShowsTheStatusInCapitals();
    }
}
