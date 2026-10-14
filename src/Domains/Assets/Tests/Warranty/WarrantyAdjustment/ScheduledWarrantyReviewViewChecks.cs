namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class ScheduledWarrantyReviewViewChecks
{
    public static void ScheduledWarrantyReviewViewShowsTheStatusInCapitals()
    {
        var item = ScheduledWarrantyReview.NewScheduledWarrantyReview("code-452", "title-464");
        var view = ScheduledWarrantyReviewView.ToScheduledWarrantyReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledWarrantyReviewView.ToScheduledWarrantyReviewViews(new List<ScheduledWarrantyReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledWarrantyReviewViewShowsTheStatusInCapitals();
    }
}
