namespace Acme.Workforce.Compliance.Tests.Exception.Review;

using Acme.Workforce.Compliance.Exception.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ExceptionReviewViewChecks
{
    public static void ExceptionReviewViewShowsTheStatusInCapitals()
    {
        var item = ExceptionReview.NewExceptionReview("code-441", "name-735");
        var view = ExceptionReviewView.ToExceptionReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionReviewView.ToExceptionReviewViews(new List<ExceptionReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionReviewViewShowsTheStatusInCapitals();
    }
}
