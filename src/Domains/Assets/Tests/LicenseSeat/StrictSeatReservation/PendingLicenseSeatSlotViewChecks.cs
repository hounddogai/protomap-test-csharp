namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class PendingLicenseSeatSlotViewChecks
{
    public static void PendingLicenseSeatSlotViewShowsTheStatusInCapitals()
    {
        var item = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot("code-752", "label-510");
        var view = PendingLicenseSeatSlotView.ToPendingLicenseSeatSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingLicenseSeatSlotView.ToPendingLicenseSeatSlotViews(new List<PendingLicenseSeatSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingLicenseSeatSlotViewShowsTheStatusInCapitals();
    }
}
