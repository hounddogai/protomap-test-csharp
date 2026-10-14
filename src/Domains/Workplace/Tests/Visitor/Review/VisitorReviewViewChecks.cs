namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VisitorReviewViewChecks
{
    public static void VisitorReviewViewShowsTheStatusInCapitals()
    {
        var item = VisitorReview.NewVisitorReview("code-35", "name-138");
        var view = VisitorReviewView.ToVisitorReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorReviewView.ToVisitorReviewViews(new List<VisitorReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorReviewViewShowsTheStatusInCapitals();
    }
}
