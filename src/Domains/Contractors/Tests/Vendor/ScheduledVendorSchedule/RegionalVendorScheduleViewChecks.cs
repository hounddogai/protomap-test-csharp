namespace Acme.Workforce.Contractors.Tests.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

/// <summary>Checks of scheduled vendor schedule code, run by the project's runner.</summary>
public static class RegionalVendorScheduleViewChecks
{
    public static void RegionalVendorScheduleViewShowsTheStatusInCapitals()
    {
        var item = RegionalVendorSchedule.NewRegionalVendorSchedule("code-919", "title-847");
        var view = RegionalVendorScheduleView.ToRegionalVendorScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalVendorScheduleView.ToRegionalVendorScheduleViews(new List<RegionalVendorSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalVendorScheduleViewShowsTheStatusInCapitals();
    }
}
