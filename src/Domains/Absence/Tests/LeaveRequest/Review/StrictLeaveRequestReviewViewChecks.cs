namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictLeaveRequestReviewViewChecks
{
    public static void StrictLeaveRequestReviewViewShowsTheStatusInCapitals()
    {
        var item = StrictLeaveRequestReview.NewStrictLeaveRequestReview("code-718", "title-546");
        var view = StrictLeaveRequestReviewView.ToStrictLeaveRequestReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictLeaveRequestReviewView.ToStrictLeaveRequestReviewViews(new List<StrictLeaveRequestReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictLeaveRequestReviewViewShowsTheStatusInCapitals();
    }
}
