namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class TieredVendorScheduleViewChecks
{
    public static void TieredVendorScheduleViewShowsTheStatusInCapitals()
    {
        var item = TieredVendorSchedule.NewTieredVendorSchedule("code-369", "label-132");
        var view = TieredVendorScheduleView.ToTieredVendorScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredVendorScheduleView.ToTieredVendorScheduleViews(new List<TieredVendorSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredVendorScheduleViewShowsTheStatusInCapitals();
    }
}
