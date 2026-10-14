namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorReviewChecks
{
    public static void VendorReviewStartsInCreated()
    {
        var item = VendorReview.NewVendorReview("code-524", "title-195");
        Check.Equal(item.Status, VendorReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorReviewMovesToValidated()
    {
        var item = VendorReview.NewVendorReview("code-524", "title-195");
        try
        {
            item.MoveTo(VendorReviewStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorReviewStatus.Validated, "status");
    }

    public static void VendorReviewRejectsMovesFromRetired()
    {
        var item = VendorReview.NewVendorReview("code-524", "title-195");
        item.Status = VendorReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(VendorReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        VendorReviewStartsInCreated();
        VendorReviewMovesToValidated();
        VendorReviewRejectsMovesFromRetired();
    }
}
