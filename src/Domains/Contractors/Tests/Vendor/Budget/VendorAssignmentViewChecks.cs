namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorAssignmentViewChecks
{
    public static void VendorAssignmentViewShowsTheStatusInCapitals()
    {
        var item = VendorAssignment.NewVendorAssignment("code-126", "title-886");
        var view = VendorAssignmentView.ToVendorAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorAssignmentView.ToVendorAssignmentViews(new List<VendorAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorAssignmentViewShowsTheStatusInCapitals();
    }
}
