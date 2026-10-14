namespace Acme.Workforce.Contractors.Tests.Vendor.Score;

using Acme.Workforce.Contractors.Vendor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveVendorAssignmentViewChecks
{
    public static void AdaptiveVendorAssignmentViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveVendorAssignment.NewAdaptiveVendorAssignment("code-657", "title-8");
        var view = AdaptiveVendorAssignmentView.ToAdaptiveVendorAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveVendorAssignmentView.ToAdaptiveVendorAssignmentViews(new List<AdaptiveVendorAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveVendorAssignmentViewShowsTheStatusInCapitals();
    }
}
