namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanReviewViewChecks
{
    public static void ReturnPlanReviewViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanReview.NewReturnPlanReview("code-929", "name-130");
        var view = ReturnPlanReviewView.ToReturnPlanReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanReviewView.ToReturnPlanReviewViews(new List<ReturnPlanReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanReviewViewShowsTheStatusInCapitals();
    }
}
