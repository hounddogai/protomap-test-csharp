namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class GlobalVendorReviewViewChecks
{
    public static void GlobalVendorReviewViewShowsTheStatusInCapitals()
    {
        var item = GlobalVendorReview.NewGlobalVendorReview("code-830", "name-195");
        var view = GlobalVendorReviewView.ToGlobalVendorReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalVendorReviewView.ToGlobalVendorReviewViews(new List<GlobalVendorReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalVendorReviewViewShowsTheStatusInCapitals();
    }
}
