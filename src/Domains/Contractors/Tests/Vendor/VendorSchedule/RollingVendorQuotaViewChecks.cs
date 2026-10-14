namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class RollingVendorQuotaViewChecks
{
    public static void RollingVendorQuotaViewShowsTheStatusInCapitals()
    {
        var item = RollingVendorQuota.NewRollingVendorQuota("code-268", "label-803");
        var view = RollingVendorQuotaView.ToRollingVendorQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingVendorQuotaView.ToRollingVendorQuotaViews(new List<RollingVendorQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingVendorQuotaViewShowsTheStatusInCapitals();
    }
}
