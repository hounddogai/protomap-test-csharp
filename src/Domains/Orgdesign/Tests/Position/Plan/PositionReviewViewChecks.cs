namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PositionReviewViewChecks
{
    public static void PositionReviewViewShowsTheStatusInCapitals()
    {
        var item = PositionReview.NewPositionReview("code-634", "name-584");
        var view = PositionReviewView.ToPositionReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionReviewView.ToPositionReviewViews(new List<PositionReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionReviewViewShowsTheStatusInCapitals();
    }
}
