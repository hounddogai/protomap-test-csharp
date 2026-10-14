namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBatch;

using Acme.Workforce.Contractors.Vendor.VendorBatch;

/// <summary>Checks of vendor batch code, run by the project's runner.</summary>
public static class LegacyVendorAssignmentViewChecks
{
    public static void LegacyVendorAssignmentViewShowsTheStatusInCapitals()
    {
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("code-975", "label-870");
        var view = LegacyVendorAssignmentView.ToLegacyVendorAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyVendorAssignmentView.ToLegacyVendorAssignmentViews(new List<LegacyVendorAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyVendorAssignmentViewShowsTheStatusInCapitals();
    }
}
