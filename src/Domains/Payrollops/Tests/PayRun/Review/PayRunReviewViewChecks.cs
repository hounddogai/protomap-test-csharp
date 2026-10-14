namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunReviewViewChecks
{
    public static void PayRunReviewViewShowsTheStatusInCapitals()
    {
        var item = PayRunReview.NewPayRunReview("code-995", "title-392");
        var view = PayRunReviewView.ToPayRunReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunReviewView.ToPayRunReviewViews(new List<PayRunReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunReviewViewShowsTheStatusInCapitals();
    }
}
