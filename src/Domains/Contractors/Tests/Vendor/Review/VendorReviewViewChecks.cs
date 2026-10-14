namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorReviewViewChecks
{
    public static void VendorReviewViewShowsTheStatusInCapitals()
    {
        var item = VendorReview.NewVendorReview("code-524", "title-195");
        var view = VendorReviewView.ToVendorReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorReviewView.ToVendorReviewViews(new List<VendorReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorReviewViewShowsTheStatusInCapitals();
    }
}
