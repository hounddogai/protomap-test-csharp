namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAdjustment;

using Acme.Workforce.Orgdesign.Position.PositionAdjustment;

/// <summary>Checks of position adjustment code, run by the project's runner.</summary>
public static class ManualPositionReviewViewChecks
{
    public static void ManualPositionReviewViewShowsTheStatusInCapitals()
    {
        var item = ManualPositionReview.NewManualPositionReview("code-669", "name-271");
        var view = ManualPositionReviewView.ToManualPositionReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPositionReviewView.ToManualPositionReviewViews(new List<ManualPositionReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPositionReviewViewShowsTheStatusInCapitals();
    }
}
