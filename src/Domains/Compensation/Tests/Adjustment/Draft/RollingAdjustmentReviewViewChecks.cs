namespace Acme.Workforce.Compensation.Tests.Adjustment.Draft;

using Acme.Workforce.Compensation.Adjustment.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RollingAdjustmentReviewViewChecks
{
    public static void RollingAdjustmentReviewViewShowsTheStatusInCapitals()
    {
        var item = RollingAdjustmentReview.NewRollingAdjustmentReview("code-55", "label-594");
        var view = RollingAdjustmentReviewView.ToRollingAdjustmentReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingAdjustmentReviewView.ToRollingAdjustmentReviewViews(new List<RollingAdjustmentReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingAdjustmentReviewViewShowsTheStatusInCapitals();
    }
}
