namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualLicenseSeatSlotViewChecks
{
    public static void ManualLicenseSeatSlotViewShowsTheStatusInCapitals()
    {
        var item = ManualLicenseSeatSlot.NewManualLicenseSeatSlot("code-291", "name-887");
        var view = ManualLicenseSeatSlotView.ToManualLicenseSeatSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualLicenseSeatSlotView.ToManualLicenseSeatSlotViews(new List<ManualLicenseSeatSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualLicenseSeatSlotViewShowsTheStatusInCapitals();
    }
}
