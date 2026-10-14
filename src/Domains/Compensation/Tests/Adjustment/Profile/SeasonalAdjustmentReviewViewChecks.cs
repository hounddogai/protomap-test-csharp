namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalAdjustmentReviewViewChecks
{
    public static void SeasonalAdjustmentReviewViewShowsTheStatusInCapitals()
    {
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("code-600", "title-42");
        var view = SeasonalAdjustmentReviewView.ToSeasonalAdjustmentReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalAdjustmentReviewView.ToSeasonalAdjustmentReviewViews(new List<SeasonalAdjustmentReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentReviewViewShowsTheStatusInCapitals();
    }
}
