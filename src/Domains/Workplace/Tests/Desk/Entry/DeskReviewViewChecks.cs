namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskReviewViewChecks
{
    public static void DeskReviewViewShowsTheStatusInCapitals()
    {
        var item = DeskReview.NewDeskReview("code-550", "name-414");
        var view = DeskReviewView.ToDeskReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskReviewView.ToDeskReviewViews(new List<DeskReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskReviewViewShowsTheStatusInCapitals();
    }
}
