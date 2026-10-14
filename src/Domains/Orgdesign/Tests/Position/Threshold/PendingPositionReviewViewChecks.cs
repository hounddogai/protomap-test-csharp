namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PendingPositionReviewViewChecks
{
    public static void PendingPositionReviewViewShowsTheStatusInCapitals()
    {
        var item = PendingPositionReview.NewPendingPositionReview("code-211", "name-282");
        var view = PendingPositionReviewView.ToPendingPositionReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPositionReviewView.ToPendingPositionReviewViews(new List<PendingPositionReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPositionReviewViewShowsTheStatusInCapitals();
    }
}
