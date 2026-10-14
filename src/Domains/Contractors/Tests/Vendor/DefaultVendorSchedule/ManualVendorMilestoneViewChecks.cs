namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class ManualVendorMilestoneViewChecks
{
    public static void ManualVendorMilestoneViewShowsTheStatusInCapitals()
    {
        var item = ManualVendorMilestone.NewManualVendorMilestone("code-137", "name-799");
        var view = ManualVendorMilestoneView.ToManualVendorMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualVendorMilestoneView.ToManualVendorMilestoneViews(new List<ManualVendorMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualVendorMilestoneViewShowsTheStatusInCapitals();
    }
}
