namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanReviewViewChecks
{
    public static void SuccessionPlanReviewViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanReview.NewSuccessionPlanReview("code-455", "title-185");
        var view = SuccessionPlanReviewView.ToSuccessionPlanReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanReviewView.ToSuccessionPlanReviewViews(new List<SuccessionPlanReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanReviewViewShowsTheStatusInCapitals();
    }
}
